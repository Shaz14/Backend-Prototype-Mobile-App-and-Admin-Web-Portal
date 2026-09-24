using MediatR;
using Microsoft.EntityFrameworkCore;
using DHAFacilitationAPIs.Application.Common.Interfaces;

namespace DHAFacilitationAPIs.Application.Feature.RoomBooking.Queries.Rooms;

public record GetAvailableRoomsQuery(
    Guid ClubId,
    Guid RoomCategoryId,
    Guid ResidenceTypeId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    string BookingType
) : IRequest<List<AvailableRoomDto>>;

public class GetAvailableRoomsQueryHandler : IRequestHandler<GetAvailableRoomsQuery, List<AvailableRoomDto>>
{
    private readonly IOLMRSApplicationDbContext _context;

    public GetAvailableRoomsQueryHandler(IOLMRSApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailableRoomDto>> Handle(GetAvailableRoomsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Rooms
            .Where(r => r.ClubId == request.ClubId &&
                        r.RoomCategoryId == request.RoomCategoryId &&
                        r.ResidenceTypeId == request.ResidenceTypeId &&
                        !_context.RoomAvailabilities.Any(rb =>
                            rb.RoomId == r.Id &&
                            rb.FromDate < request.CheckOutDate &&
                            rb.ToDate > request.CheckInDate))
            .Select(r => new AvailableRoomDto
            {
                RoomId = r.Id,
                Name = r.Name ?? "",
                No = r.No,
                Price = _context.RoomCharges
                    .Where(rc => rc.RoomId == r.Id && rc.BookingType == request.BookingType)
                    .Select(rc => rc.Charges)
                    .FirstOrDefault(),
                Ratings = _context.RoomRatings
                        .Where(rate => rate.RoomId == r.Id)
                        .Select(rate => rate.RoomRatings)
                        .FirstOrDefault(),
                Images = _context.RoomImages
                    .Where(img => img.RoomId == r.Id)
                    .Select(img => img.ImageURL)
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
