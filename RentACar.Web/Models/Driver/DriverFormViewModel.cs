using System.ComponentModel.DataAnnotations;

namespace RentACar.Web.Models.Driver;

public class DriverFormViewModel
{
    public int? DriverId { get; set; }

    [Required]
    public string AspNetUserId { get; set; } = string.Empty;

    [Required]
    public string DisplayName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; }
    public bool IsAvailableManual { get; set; }
}
