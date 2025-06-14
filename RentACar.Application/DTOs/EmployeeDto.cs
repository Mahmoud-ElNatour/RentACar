using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;
        public decimal? Salary { get; set; }
        public string aspNetUserId { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public string Email { get; internal set; }
        public string username { get; internal set; }
        public string? PhoneNumber { get; set; }
    }
}