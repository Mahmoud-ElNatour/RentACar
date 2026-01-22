namespace RentACar.Web.Models.Driver;

public class DriverBookingApiItem
{
    public int BookingId { get; set; }
    public string? CustomerName { get; set; }
    public string? CarModel { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Status { get; set; }
    public string? PickupAddress { get; set; }
}
