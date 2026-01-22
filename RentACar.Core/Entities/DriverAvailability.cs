using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

public partial class DriverAvailability
{
    [Key]
    [Column("driverAvailabilityID")]
    public int DriverAvailabilityId { get; set; }

    [Column("driverID")]
    public int DriverId { get; set; }

    [Column("startTime")]
    public DateTime StartTime { get; set; }

    [Column("endTime")]
    public DateTime EndTime { get; set; }

    [Column("isRecurringWeekly")]
    public bool IsRecurringWeekly { get; set; }

    [ForeignKey("DriverId")]
    [InverseProperty("DriverAvailabilities")]
    public virtual Driver Driver { get; set; } = null!;
}
