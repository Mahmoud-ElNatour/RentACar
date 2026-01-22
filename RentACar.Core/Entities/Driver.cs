using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

public partial class Driver
{
    [Key]
    [Column("driverID")]
    public int DriverId { get; set; }

    [Column("aspNetUserId")]
    [StringLength(450)]
    public string AspNetUserId { get; set; } = null!;

    [Column("displayName")]
    [StringLength(100)]
    public string DisplayName { get; set; } = null!;

    [Column("phoneNumber")]
    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    [Column("isActive")]
    public bool IsActive { get; set; }

    [Column("isAvailableManual")]
    public bool IsAvailableManual { get; set; }

    [InverseProperty("Driver")]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    [InverseProperty("Driver")]
    public virtual ICollection<DriverAvailability> DriverAvailabilities { get; set; } = new List<DriverAvailability>();

    [InverseProperty("Driver")]
    public virtual ICollection<DriverLocation> DriverLocations { get; set; } = new List<DriverLocation>();

    [ForeignKey("AspNetUserId")]
    [InverseProperty("Driver")]
    public virtual AspNetUser User { get; set; } = null!;
}
