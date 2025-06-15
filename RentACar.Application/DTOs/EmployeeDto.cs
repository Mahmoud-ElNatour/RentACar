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

        [Required]
        public string username { get; set; } = null!;

        public string? PhoneNumber { get; set; }
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

        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public string? PhoneNumber { get; set; }
    }
}