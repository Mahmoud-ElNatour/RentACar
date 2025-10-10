using System;

namespace RentACar.Application.DTOs
{
    public class BlacklistDisplayDto
    {
        public int BlacklistId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateOnly DateBlocked { get; set; }
        public int EmployeeDoneBlacklistId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;

        /// <summary>
        /// Type of the user being blacklisted (Customer or Employee)
        /// </summary>
        public string UserType { get; set; } = string.Empty;
    }
}
