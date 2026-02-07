using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RentACar.Application.DTOs
{
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        public decimal? Salary { get; set; }

        [JsonIgnore]
        public string aspNetUserId { get; set; } = string.Empty;

        public string? Address { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        
        public string username { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public List<string> Roles { get; set; } = new();

        public int? DriverId { get; set; }
        public string? DriverCode { get; set; }
        public string? DriverFullName { get; set; }
        public string? DriverPhone { get; set; }
        public string? DriverEmail { get; set; }
        public decimal? DriverDailyFeePerDay { get; set; }
        public decimal? DriverRating { get; set; }
        public string? DriverLicenseNumber { get; set; }
        public DateOnly? DriverLicenseExpiry { get; set; }
        public string? DriverLanguages { get; set; }
        public string? DriverNotes { get; set; }
        public bool DriverIsActive { get; set; } = true;
        public DateTime? DriverCreatedAt { get; set; }
        public DateTime? DriverUpdatedAt { get; set; }
        public List<int> AllowedCategoryIds { get; set; } = new();
    }


    public class EmployeeCreateDTO
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;
        public decimal? Salary { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        //[Required]
        //public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        // Added for multi-role and driver support
        public List<string>? Roles { get; set; }

        public string? DriverCode { get; set; }
        public string? DriverFullName { get; set; }
        public string? DriverPhone { get; set; }
        public string? DriverEmail { get; set; }
        public decimal? DriverDailyFeePerDay { get; set; }
        public decimal? DriverRating { get; set; }
        public string? DriverLicenseNumber { get; set; }
        public DateOnly? DriverLicenseExpiry { get; set; }
        public string? DriverLanguages { get; set; }
        public string? DriverNotes { get; set; }
        public bool DriverIsActive { get; set; } = true;
        public List<int> AllowedCategoryIds { get; set; } = new();
    }

    public class EmployeeDisplayDto
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal? Salary { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; } = "Unknown";
    }

}
