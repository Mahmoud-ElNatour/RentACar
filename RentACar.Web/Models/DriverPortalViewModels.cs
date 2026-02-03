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
    public DateOnly MonthStart { get; set; }
    public DateOnly MonthEnd { get; set; }
    public List<DriverScheduleDayViewModel> Days { get; set; } = new();
    public List<DriverAvailabilityItemViewModel> Availability { get; set; } = new();
    public List<DriverScheduleBookingItemViewModel> Bookings { get; set; } = new();
    public List<DriverPortalBookingViewModel> UpcomingBookings { get; set; } = new();
}

public class DriverBookingDetailsViewModel
{
    public int BookingId { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
    public string TripStatus { get; set; } = string.Empty;
    public string CarName { get; set; } = string.Empty;
    public string CarPlate { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PickupLocationLabel { get; set; } = string.Empty;
    public double? PickupLatitude { get; set; }
    public double? PickupLongitude { get; set; }
    public double? DriverLatitude { get; set; }
    public double? DriverLongitude { get; set; }
    public DateTime? PickupDateTime { get; set; }
    public string DriverCode { get; set; } = string.Empty;
    public int DriverId { get; set; }
}

public class DriverPortalBookingViewModel
{
    public int BookingId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PickupLocationLabel { get; set; } = string.Empty;
    public double? PickupLatitude { get; set; }
    public double? PickupLongitude { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
    public string? TripStatus { get; set; }
}

public class DriverScheduleDayViewModel
{
    public DateOnly Date { get; set; }
    public bool IsToday { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool HasAvailability { get; set; }
    public bool HasBookings { get; set; }
    public List<DriverScheduleBookingItemViewModel> Bookings { get; set; } = new();
    public List<DriverAvailabilityItemViewModel> AvailabilityBlocks { get; set; } = new();
}

public class DriverScheduleBookingItemViewModel
{
    public int BookingId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime? PickupDateTime { get; set; }
    public string PickupLocationLabel { get; set; } = string.Empty;
    public string BookingStatus { get; set; } = string.Empty;
    public string? TripStatus { get; set; }
}

public class DriverAvailabilityItemViewModel
{
    public int DriverAvailabilityId { get; set; }
    public DateOnly Date { get; set; }
    public bool IsAvailable { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }

    // Legacy fields
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public bool? IsRecurringWeekly { get; set; }
}
