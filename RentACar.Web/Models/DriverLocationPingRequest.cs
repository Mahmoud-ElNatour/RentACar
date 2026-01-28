using System.ComponentModel.DataAnnotations;

namespace RentACar.Web.Models;

public class DriverLocationPingRequest
{
    [Required]
    public int BookingId { get; set; }

    [Required]
    public decimal Latitude { get; set; }

    [Required]
    public decimal Longitude { get; set; }

    public decimal? Speed { get; set; }
    public decimal? Heading { get; set; }
    public decimal? Accuracy { get; set; }
    public int? Battery { get; set; }
}
