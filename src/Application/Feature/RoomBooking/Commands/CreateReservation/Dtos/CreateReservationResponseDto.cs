using System;
using System.Collections.Generic;

namespace DHAFacilitationAPIs.Application.Feature.RoomBooking.Commands.CreateReservation;
public class CreateReservationResponseDto
{
    public Guid ReservationId { get; set; }
    public string OneBillId { get; set; } = string.Empty;
    public decimal RoomsAmount { get; set; }
    public decimal Taxes { get; set; }
    public decimal Discounts { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime ExpiresAt { get; set; }
}
