using System.ComponentModel.DataAnnotations;

namespace RentACar.Web.Models;

public class AssignDriverRequest
{
    [Required]
    public int BookingId { get; set; }

    public int? DriverId { get; set; }
}
