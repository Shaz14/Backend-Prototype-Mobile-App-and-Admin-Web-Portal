using System;
using System.Threading;
using System.Threading.Tasks;
using DHAFacilitationAPIs.Application.Common.Interfaces;
using DHAFacilitationAPIs.Application.Feature.Room.Queries.GetAllRooms;
using DHAFacilitationAPIs.Application.ViewModels;
using DHAFacilitationAPIs.Domain.Entities;
using DHAFacilitationAPIs.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Dapper.SqlMapper;

namespace DHAFacilitationAPIs.Application.Feature.RoomBooking.Commands.CreateReservation;

public record CreateReservationCommand(CreateReservationDto Reservation) : IRequest<CreateReservationResponseDto>;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, CreateReservationResponseDto>
{
    private readonly IOLMRSApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISmartPayService _smartPayService;

    public CreateReservationCommandHandler(IOLMRSApplicationDbContext context, UserManager<ApplicationUser> userManager, ISmartPayService smartPayService)
    {
        _context = context;
        _userManager = userManager;
        _smartPayService = smartPayService;
    }

    public async Task<CreateReservationResponseDto> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Reservation;

        if (dto.UserId == Guid.Empty)
            throw new InvalidOperationException("UserId must be provided to create a reservation.");

        if (dto.Rooms == null || !dto.Rooms.Any())
            throw new InvalidOperationException("At least one room must be provided.");

        // Calculate totals
        decimal roomsAmount = dto.Rooms.Sum(r =>
        {
            var totalDays = (r.ToDate.Date - r.FromDate.Date).Days;
            if (totalDays <= 0)
                throw new Exception("ToDate must be after FromDate");

            return totalDays * r.PricePerNight;
        });
        decimal taxes = 0m;
        decimal discounts = 0m;
        decimal totalAmount = roomsAmount + taxes - discounts;

        decimal depositPercent = 30m;
        decimal depositAmount = Math.Round(totalAmount * (depositPercent / 100m), 2);

        var reservation = new Reservation
        {
            UserId = dto.UserId,
            ClubId = dto.ClubId,
            Status = ReservationStatus.AwaitingPayment,
            RoomsAmount = roomsAmount,
            Taxes = taxes,
            Discounts = discounts,
            TotalAmount = totalAmount,
            DepositPercentRequired = depositPercent,
            DepositAmountRequired = depositAmount
        };

        // Check Guest CNIC if guest data provided
        if (dto.Guest != null && !string.IsNullOrWhiteSpace(dto.Guest.CNICOrPassport))
        {
            var existingGuest = await _context.BookingGuests
                .FirstOrDefaultAsync(g => g.CNICOrPassport == dto.Guest.CNICOrPassport, cancellationToken);

            if (existingGuest != null)
            {
                reservation.GuestId = existingGuest.Id;
            }
            else
            {
                // No match → create new guest
                var newGuest = new BookingGuest
                {
                    FullName = dto.Guest.FullName,
                    CNICOrPassport = dto.Guest.CNICOrPassport,
                    Phone = dto.Guest.Phone,
                    Email = dto.Guest.Email,
                    Address = dto.Guest.Address
                };

                _context.BookingGuests.Add(newGuest);
                await _context.SaveChangesAsync(cancellationToken); // Save immediately to get ID

                reservation.GuestId = newGuest.Id;
            }
        }

        // Add reservation rooms
        foreach (var room in dto.Rooms)
        {
            var totalDays = (room.ToDate.Date - room.FromDate.Date).Days;
            if (totalDays <= 0)
                throw new Exception("ToDate must be after FromDate");

            var subTotal = totalDays * room.PricePerNight;
            reservation.ReservationRooms.Add(new ReservationRoom
            {
                RoomId = room.RoomId,
                FromDate = room.FromDate,
                ToDate = room.ToDate,
                PricePerNight = room.PricePerNight,
                Subtotal = subTotal
            });
        }

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);

        // SmartPay Integration
        var club = await _context.Clubs.FirstOrDefaultAsync(c => c.Id == dto.ClubId, cancellationToken);
        if (club == null)
            throw new Exception("Club not found");

        string clubAccountCode = club.AccountCode ?? "";
        string smartPayCode = "98001"; // can move to config
        int invoiceNo = await _context.Reservations.CountAsync(cancellationToken) + 1;

        // Generate OneBill ID
        string generatedOneBillId = GenerateOneBillId(smartPayCode, clubAccountCode, invoiceNo);

        // Build PaymentIntent (RequiresPayment)
        var paymentIntent = new PaymentIntent
        {
            ReservationId = reservation.Id,
            AmountToCollect = reservation.DepositAmountRequired,
            OneBillID = generatedOneBillId,
            IsDeposit = true,
            Status = PaymentIntentStatus.RequiresPayment,
            Method = PaymentMethod.BankTransfer, 
            Provider = PaymentProvider.None,
            CreatedAt = DateTime.UtcNow
        };

        _context.PaymentIntents.Add(paymentIntent);
        await _context.SaveChangesAsync(cancellationToken);

        // Prepare SmartPay request
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId.ToString(), cancellationToken);

        if (user == null)
            throw new Exception($"User with ID {dto.UserId} not found");

        DateTime paymentExpiry = DateTime.UtcNow.AddMinutes(30);

        var billRequest = new BillUploadRequest
        {
            consumer_number = generatedOneBillId,                         // Our generated 1Bill ID
            consumer_Detail = user.Name,                                  // Customer name
            dueDate = paymentExpiry.ToString(),         // Due date in YYYYMMDD format
            expDate = paymentExpiry.ToString(),         // Expiry date in same format
            amount = depositAmount,                                       // Amount user must pay now
            lateFee = 0,                                                  // No late fee in our case
            billStatus = 1,                                               // Active
            cellNo = user.MobileNo ?? string.Empty,                       // Customer phone
            eMail = user.Email ?? string.Empty,                           // Customer email
            billReference = reservation.Id.ToString(),                    // Our internal reference
            reservedForFutureUser = ""                                    // Optional field
        };
        
        // Call SmartPay API
        BillUploadResult smartPayResult = await _smartPayService.UploadBillAsync(billRequest, cancellationToken);

        if (!(smartPayResult.ResponseCode == "00" || smartPayResult.ResponseCode == "200"))
        {
            paymentIntent.Status = PaymentIntentStatus.Failed;
            paymentIntent.Meta = smartPayResult.ToString();
            await _context.SaveChangesAsync(cancellationToken);
            throw new Exception($"SmartPay Bill Upload failed: {smartPayResult.ResponseMsg}");
        }

        // Update PaymentIntent Table and Add to Payment Table
        reservation.ExpiresAt = paymentExpiry;
        paymentIntent.ExpiresAt = paymentExpiry;
        paymentIntent.ProviderIntentId = generatedOneBillId;
        paymentIntent.Status = PaymentIntentStatus.Processing;
        var payment = new Payment
        {
            PaymentIntentId = paymentIntent.Id,
            Amount = paymentIntent.AmountToCollect,
            Status = PaymentStatus.Authorized,
        };
        _context.Payments.Add(payment);

        await _context.SaveChangesAsync(cancellationToken);

        // Return result DTO
        return new CreateReservationResponseDto
        {
            ReservationId = reservation.Id,
            OneBillId = generatedOneBillId, // replace with SmartPay response when integrated
            RoomsAmount = reservation.RoomsAmount,
            Taxes = reservation.Taxes,
            Discounts = reservation.Discounts,
            TotalAmount = reservation.TotalAmount,
            ExpiresAt = reservation.ExpiresAt
        };

        //return reservation.Id;
    }

    // (5) Helper method
    private string GenerateOneBillId(string smartPayCode, string clubAccountCode, int invoiceNo)
    {
        string yymm = DateTime.UtcNow.ToString("yyMM");
        return $"{smartPayCode}{clubAccountCode}{yymm}{invoiceNo:D6}";
    }
}
