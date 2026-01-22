using System.Collections.Generic;

namespace RentACar.Web.Models.Driver;

public class DriverDashboardApiResponse
{
    public string? DriverName { get; set; }
    public bool IsAvailableManual { get; set; }
    public bool IsOnTrip { get; set; }
    public int TodayCount { get; set; }
    public int UpcomingCount { get; set; }
    public List<DriverBookingApiItem> TodayBookings { get; set; } = new();
    public List<DriverBookingApiItem> UpcomingBookings { get; set; } = new();
}
