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
        public string Status { get; set; } = "Success"; // Kept for backward compatibility, mapped to Outcome logic

        // --- New Fields for 100% Compliance ---

        [MaxLength(100)]
        public string? TargetType { get; set; } // e.g. "Booking", "User"

        [MaxLength(100)]
        public string? TargetId { get; set; } // e.g. "105", "cs-12"

        [MaxLength(50)]
        public string? Outcome { get; set; } // "Success", "Failure", "Warning"

        public string? DetailsJson { get; set; } // Structured extra data

        public string? OldValuesJson { get; set; } // Snapshot before change

        public string? NewValuesJson { get; set; } // Snapshot after change

        [MaxLength(500)]
        public string? FailureReason { get; set; } // Why it failed

        [MaxLength(500)]
        public string? UserAgent { get; set; } // Browser/Client info

        [MaxLength(100)]
        public string? CorrelationId { get; set; } // For tracing requests
    }
}
