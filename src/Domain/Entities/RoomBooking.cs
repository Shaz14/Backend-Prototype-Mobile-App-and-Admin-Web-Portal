using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DHAFacilitationAPIs.Domain.Common;

namespace DHAFacilitationAPIs.Domain.Entities;

public class RoomBooking : BaseAuditableEntity
{
    //[Required]
    //public Guid UserId { get; set; } = default!;

    //[Required]
    //public Guid ClubId { get; set; }

    //[Required]
    //public Guid RoomId { get; set; }

    [Required]
    public Guid ReservationId { get; set; }
    public Reservation Reservation { get; set; } = default!;


    [Required]
    public Guid Booking { get; set; }

    [Required]
    public DateTime? BookingDate { get; set; } = DateTime.UtcNow;

    public BookingStatus Status { get; set; } = BookingStatus.Provisional;

    [Required]
    public DateTime? CheckInDate { get; set; }

    [Required]
    public DateTime? CheckOutDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }
}
