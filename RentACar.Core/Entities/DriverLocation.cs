using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

public partial class DriverLocation
{
    [Key]
    [Column("driverLocationID")]
    public int DriverLocationId { get; set; }

    [Column("driverID")]
    public int DriverId { get; set; }

    [Column("bookingID")]
    public int BookingId { get; set; }

    [Column("latitude", TypeName = "decimal(9, 6)")]
    public decimal Latitude { get; set; }

    [Column("longitude", TypeName = "decimal(9, 6)")]
    public decimal Longitude { get; set; }

    [Column("lastUpdatedUtc")]
    public DateTime LastUpdatedUtc { get; set; }

    [Column("isTrackingActive")]
    public bool IsTrackingActive { get; set; }

    [ForeignKey("DriverId")]
    [InverseProperty("DriverLocations")]
    public virtual Driver Driver { get; set; } = null!;

    [ForeignKey("BookingId")]
    [InverseProperty("DriverLocations")]
    public virtual Booking Booking { get; set; } = null!;
}
