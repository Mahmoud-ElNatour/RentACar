using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

[Table("Drivers")]
public partial class Driver
{
    [Key]
    [Column("driverID")]
    public int DriverId { get; set; }

    [Column("aspNetUserId")]
    [StringLength(450)]
    public string AspNetUserId { get; set; } = null!;

    [Column("employeeID")]
    public int EmployeeId { get; set; }

    [Column("driverCode")]
    [StringLength(20)]
    public string DriverCode { get; set; } = null!;

    [Column("fullName")]
    [StringLength(120)]
    public string FullName { get; set; } = null!;

    [Column("phone")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [Column("email")]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Column("isActive")]
    public bool IsActive { get; set; }

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [Column("rating", TypeName = "decimal(3, 2)")]
    public decimal? Rating { get; set; }

    [Column("licenseNumber")]
    [StringLength(50)]
    public string? LicenseNumber { get; set; }

    [Column("licenseExpiry")]
    public DateOnly? LicenseExpiry { get; set; }

    [Column("languages")]
    [StringLength(200)]
    public string? Languages { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [ForeignKey("AspNetUserId")]
    [InverseProperty("Drivers")]
    public virtual AspNetUser User { get; set; } = null!;

    [ForeignKey("EmployeeId")]
    [InverseProperty("Driver")]
    public virtual Employee Employee { get; set; } = null!;

    [InverseProperty("Driver")]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    [InverseProperty("Driver")]
    public virtual ICollection<DriverAvailability> DriverAvailabilities { get; set; } = new List<DriverAvailability>();

    [InverseProperty("Driver")]
    public virtual ICollection<DriverLocationPing> LocationPings { get; set; } = new List<DriverLocationPing>();

    [InverseProperty("Driver")]
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
