using System.ComponentModel.DataAnnotations;

namespace RentACar.Web.Models;

public class DriverBookingStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
