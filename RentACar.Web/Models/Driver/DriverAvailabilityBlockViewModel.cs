using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Web.Models.Driver;

public class DriverAvailabilityBlockViewModel
{
    public int DriverAvailabilityId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    public bool IsRecurringWeekly { get; set; }
}
