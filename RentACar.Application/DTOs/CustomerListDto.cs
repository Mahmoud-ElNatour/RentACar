using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class CustomerListDto
    {
        public int UserId { get; set; }
        
        public string Name { get; set; } = null!;
        
        public string? aspNetUserId { get; set; }
        
        public bool IsVerified { get; set; }
        
        // EXCLUDED BLOBs
        // public byte[]? DrivingLicenseFront { get; set; }
        // public byte[]? DrivingLicenseBack { get; set; }
        // public byte[]? NationalIdfront { get; set; }
        // public byte[]? NationalIdback { get; set; }
        
        public bool Isactive { get; set; }
        
        public string? Address { get; set; }
        
        public string Email { get; set; } = null!;
        
        public bool IsEmailConfirmed { get; set; }

        public string? PhoneNumber { get; set; }    }
}
