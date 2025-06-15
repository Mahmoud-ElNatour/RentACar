using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class BlacklistDto
    {
        public int BlacklistId { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = null!;

        public string? Reason { get; set; }

        [Required]
        public DateOnly DateBlocked { get; set; }

        [Required]
        public int EmployeeDoneBlacklistId { get; set; }
    }

    public class AddToBlacklistRequestDto
    {
        [Required]
        public string Identifier { get; set; } // Can be Username or UserId

        [Required]
        public string Reason { get; set; }

        public bool UseUsername { get; set; } = true; // Flag to indicate if Identifier is Username
    }

    public class RemoveFromBlacklistRequestDto
    {
        [Required]
        public string Identifier { get; set; } // Can be Username or UserId

        public bool UseUsername { get; set; } = true; // Flag to indicate if Identifier is Username
    }
}