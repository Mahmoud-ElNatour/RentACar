using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

[Table("Trips")]
public class Trip
{
    [Key]
    [Column("tripID")]
    public int TripId { get; set; }

    [Column("bookingID")]
    public int BookingId { get; set; }

    [Column("driverID")]
    public int? DriverId { get; set; }

    [Column("tripStatus")]
    public TripStatus TripStatus { get; set; } = TripStatus.Pending;

    [Column("startedAt")]
    public DateTime? StartedAt { get; set; }

    [Column("arrivedAt")]
    public DateTime? ArrivedAt { get; set; }

    [Column("tripStartedAt")]
    public DateTime? TripStartedAt { get; set; }

    [Column("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [Column("cancelledAt")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelReason")]
    [StringLength(500)]
    public string? CancelReason { get; set; }

    [Column("lastDriverLatitude", TypeName = "decimal(9, 6)")]
    public decimal? LastDriverLatitude { get; set; }

    [Column("lastDriverLongitude", TypeName = "decimal(9, 6)")]
    public decimal? LastDriverLongitude { get; set; }

    [Column("lastLocationUpdatedAt")]
    public DateTime? LastLocationUpdatedAt { get; set; }

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("BookingId")]
    [InverseProperty("Trip")]
    public virtual Booking Booking { get; set; } = null!;

    [ForeignKey("DriverId")]
    [InverseProperty("Trips")]
    public virtual Driver? Driver { get; set; }
}
