using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities
{
    public class EmailProviderSettings
    {
        [Key]
        public int Id { get; set; }

        public string ProviderType { get; set; } = "Mailjet"; // Default

        // Keys removed as per security requirement (stored in appsettings/env)
        
        public string? SenderDomain { get; set; }
        public string? DefaultReplyToEmail { get; set; }

        public bool SandboxModeEnabled { get; set; } = false;

        public int? RateLimitPerMinute { get; set; }
        public int RetryCount { get; set; } = 0;
        public int RetryDelayMinutes { get; set; } = 5;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? UpdatedByUserId { get; set; }
        
        [ForeignKey("UpdatedByUserId")]
        public virtual AspNetUser? UpdatedByUser { get; set; }
    }
}
