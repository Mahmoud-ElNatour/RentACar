using System;
using System.Collections.Generic;

namespace RentACar.Web.Models;

public class DriverDashboardViewModel
{
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string DriverCode { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public List<DriverPortalBookingViewModel> TodayBookings { get; set; } = new();
}

public class DriverScheduleViewModel
{
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public List<DriverAvailabilityItemViewModel> Availability { get; set; } = new();
    public List<DriverPortalBookingViewModel> UpcomingBookings { get; set; } = new();
}

public class DriverBookingDetailsViewModel
{
    public int BookingId { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
    public string CarName { get; set; } = string.Empty;
    public string CarPlate { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public DateTime? PickupDateTime { get; set; }
    public string DriverCode { get; set; } = string.Empty;
}

public class DriverPortalBookingViewModel
{
    public int BookingId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
}

public class DriverAvailabilityItemViewModel
{
    public int DriverAvailabilityId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public bool IsRecurringWeekly { get; set; }
    public bool IsAvailable { get; set; }
}
