using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

[Table("DriverLocationPings")]
public partial class DriverLocationPing
{
    [Key]
    [Column("driverLocationPingID")]
    public int DriverLocationPingId { get; set; }

    [Column("bookingID")]
    public int BookingId { get; set; }

    [Column("driverID")]
    public int DriverId { get; set; }

    [Column("latitude", TypeName = "decimal(9, 6)")]
    public decimal Latitude { get; set; }

    [Column("longitude", TypeName = "decimal(9, 6)")]
    public decimal Longitude { get; set; }

    [Column("speed", TypeName = "decimal(10, 2)")]
    public decimal? Speed { get; set; }

    [Column("heading", TypeName = "decimal(10, 2)")]
    public decimal? Heading { get; set; }

    [Column("accuracyMeters", TypeName = "decimal(10, 2)")]
    public decimal? AccuracyMeters { get; set; }

    [Column("batteryPercent")]
    public int? BatteryPercent { get; set; }

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("BookingId")]
    [InverseProperty("DriverLocationPings")]
    public virtual Booking Booking { get; set; } = null!;

    [ForeignKey("DriverId")]
    [InverseProperty("LocationPings")]
    public virtual Driver Driver { get; set; } = null!;
}
