using System;

namespace RentACar.Application.DTOs
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string ActorName { get; set; } = string.Empty;
        public string ActorRole { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? Device { get; set; }
        public string Status { get; set; } = string.Empty;
        
        // Extended properties
        public string? TargetType { get; set; }
        public string? TargetId { get; set; }
        public string? Outcome { get; set; }
        public string? DetailsJson { get; set; }
        public string? OldValuesJson { get; set; }
        public string? NewValuesJson { get; set; }
        public string? FailureReason { get; set; }
    }
}
