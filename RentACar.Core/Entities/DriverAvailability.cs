using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

[Table("DriverAvailabilities")]
public partial class DriverAvailability
{
    [Key]
    [Column("driverAvailabilityID")]
    public int DriverAvailabilityId { get; set; }

    [Column("driverID")]
    public int DriverId { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("isAvailable")]
    public bool IsAvailable { get; set; }

    [Column("startTime")]
    public TimeOnly? StartTime { get; set; }

    [Column("endTime")]
    public TimeOnly? EndTime { get; set; }

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    // Legacy fields for backward compatibility
    [Column("startDateTime")]
    public DateTime? StartDateTime { get; set; }

    [Column("endDateTime")]
    public DateTime? EndDateTime { get; set; }

    [Column("isRecurringWeekly")]
    public bool? IsRecurringWeekly { get; set; }

    [ForeignKey("DriverId")]
    [InverseProperty("DriverAvailabilities")]
    public virtual Driver Driver { get; set; } = null!;
}
