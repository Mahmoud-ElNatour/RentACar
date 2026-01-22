using System;

namespace RentACar.Web.Models.Driver;

public class DriverBookingListItemViewModel
{
    public int BookingId { get; set; }
    public string? CustomerName { get; set; }
    public string? CarModel { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Status { get; set; }
    public string? PickupAddress { get; set; }
}
