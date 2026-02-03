using System.ComponentModel.DataAnnotations;

namespace RentACar.Web.Models
{
    public class EmployeeDoneUpsertDto
    {
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public decimal? Salary { get; set; }

        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        public List<string> Roles { get; set; } = new();

        public string? Password { get; set; }

        public int? DriverId { get; set; }
        public string? DriverCode { get; set; }
        public string? DriverFullName { get; set; }
        public string? DriverPhone { get; set; }
        public string? DriverEmail { get; set; }
        public decimal? DriverRating { get; set; }
        public string? DriverLicenseNumber { get; set; }
        public DateOnly? DriverLicenseExpiry { get; set; }
        public string? DriverLanguages { get; set; }
        public string? DriverNotes { get; set; }
        public bool DriverIsActive { get; set; } = true;
    }
}
