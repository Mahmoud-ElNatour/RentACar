using System;

namespace RentACar.Web.Models;

public class CustomerTrackingViewModel
{
    public int BookingId { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string DriverCode { get; set; } = string.Empty;
    public string CarName { get; set; } = string.Empty;
    public string CarPlate { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public DateTime? PickupDateTime { get; set; }
    public decimal? LastLatitude { get; set; }
    public decimal? LastLongitude { get; set; }
    public DateTime? LastPingAt { get; set; }
    public double? PickupLatitude { get; set; }
    public double? PickupLongitude { get; set; }
}
