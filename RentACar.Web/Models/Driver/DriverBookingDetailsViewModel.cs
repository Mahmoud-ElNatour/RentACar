using System;

namespace RentACar.Web.Models.Driver;

public class DriverBookingDetailsViewModel
{
    public int BookingId { get; set; }
    public string? BookingStatus { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CarModel { get; set; }
    public string? CarPlateNumber { get; set; }
    public string? PickupAddress { get; set; }
    public decimal? PickupLat { get; set; }
    public decimal? PickupLng { get; set; }
    public bool IsTrackingActive { get; set; }
    public DateTime? LastTrackingUpdateUtc { get; set; }
}
