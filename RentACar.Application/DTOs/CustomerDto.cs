using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class CustomerDTO
    {
        public int UserId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public string aspNetUserId { get; set; }
        public bool IsVerified { get; set; }
        public byte[]? DrivingLicenseFront { get; set; }
        public byte[]? DrivingLicenseBack { get; set; }
        public byte[]? NationalIdfront { get; set; }
        public byte[]? NationalIdback { get; set; }
        public bool Isactive { get; set; }
        public string? Address { get; set; }
        public string Email { get; internal set; }
        public string username { get; internal set; }
        public string? PhoneNumber { get; set; }
    }
    public class CustomerCreateDTO
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public string Address { get; set; }

        [Required]
        public byte[] DrivingLicenseFront { get; set; }

        [Required]
        public byte[] DrivingLicenseBack { get; set; }

        [Required]
        public byte[] NationalIdfront { get; set; }

        [Required]
        public byte[] NationalIdback { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }
    }

}