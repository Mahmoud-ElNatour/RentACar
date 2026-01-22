using System.Collections.Generic;

namespace RentACar.Web.Models.Driver;

public class DriverDashboardViewModel
{
    public string? DriverName { get; set; }
    public bool IsAvailableManual { get; set; }
    public bool IsOnTrip { get; set; }
    public List<DriverBookingListItemViewModel> TodayBookings { get; set; } = new();
    public List<DriverBookingListItemViewModel> UpcomingBookings { get; set; } = new();
}
