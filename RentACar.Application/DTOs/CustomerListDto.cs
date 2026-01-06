using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class CustomerListDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string? aspNetUserId { get; set; }
        public bool IsVerified { get; set; }
        public bool Isactive { get; set; }
        public string? Address { get; set; }
        public string Email { get; set; }
        public string username { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
