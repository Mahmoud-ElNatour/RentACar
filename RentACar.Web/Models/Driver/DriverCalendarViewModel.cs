using System;
using System.Collections.Generic;

namespace RentACar.Web.Models.Driver;

public class DriverCalendarViewModel
{
    public DateTime Month { get; set; }
    public List<DriverBookingListItemViewModel> Bookings { get; set; } = new();
    public List<DriverAvailabilityBlockViewModel> AvailabilityBlocks { get; set; } = new();
}
