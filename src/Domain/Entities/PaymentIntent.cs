using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class PaymentIntent : BaseAuditableEntity
{
    // Link to Reservation during hold; after conversion we can also link to Booking
    [Required] public Guid? ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public Guid? BookingId { get; set; }
    public RoomBooking? Booking { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal AmountToCollect { get; set; }          // deposit or full
    public string? OneBillID { get; set; }
    public bool IsDeposit { get; set; } = true;
    public PaymentIntentStatus Status { get; set; } = PaymentIntentStatus.RequiresPayment;

    public PaymentMethod Method { get; set; }
    public PaymentProvider Provider { get; set; } = PaymentProvider.None;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }              // optional payment window

    [MaxLength(100)] public string? ProviderIntentId { get; set; } // gateway identifier
    public string? Meta { get; set; }                               // JSON payload etc.

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
