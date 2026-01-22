using System;

namespace RentACar.Web.Models;

public class TrackDriverViewModel
{
    public int BookingId { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public decimal? DriverLat { get; set; }
    public decimal? DriverLng { get; set; }
    public DateTime? LastUpdatedUtc { get; set; }
    public string? PickupAddress { get; set; }
    public decimal? PickupLat { get; set; }
    public decimal? PickupLng { get; set; }
    public bool IsTrackingActive { get; set; }
}
