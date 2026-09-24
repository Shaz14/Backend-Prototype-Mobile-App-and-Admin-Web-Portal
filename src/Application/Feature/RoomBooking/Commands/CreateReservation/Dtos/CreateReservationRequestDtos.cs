using System;
using System.Collections.Generic;

namespace DHAFacilitationAPIs.Application.Feature.RoomBooking.Commands.CreateReservation;

public class CreateReservationDto
{
    public Guid UserId { get; set; }  // Now Guid instead of string
    public Guid ClubId { get; set; }
    public List<CreateReservationRoomDto> Rooms { get; set; } = new();
    public CreateGuestDto? Guest { get; set; }  // Guest info if booking for a guest
}

public class CreateReservationRoomDto
{
    public Guid RoomId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal PricePerNight { get; set; }
}

public class CreateGuestDto
{
    public string FullName { get; set; } = default!;
    public string? CNICOrPassport { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}
