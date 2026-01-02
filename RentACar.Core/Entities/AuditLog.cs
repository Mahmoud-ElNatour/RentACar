using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Core.Entities
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string ActorName { get; set; } = "System";

        [MaxLength(50)]
        public string ActorRole { get; set; } = "Unknown";

        [MaxLength(50)]
        public string Action { get; set; } = string.Empty; // Create, Update, Delete etc.

        [MaxLength(50)]
        public string Entity { get; set; } = string.Empty; // Car, Booking, etc.

        [MaxLength(50)]
        public string EntityId { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(200)]
        public string? Device { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Success";
    }
}
