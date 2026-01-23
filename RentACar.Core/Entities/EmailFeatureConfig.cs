using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities
{
    public class EmailFeatureConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FeatureKey { get; set; } // Unique Key: "VerifyEmail", "PaymentInvoice"

        [Required]
        public string FeatureName { get; set; } // "Verify Email Address"

        [Required]
        public string Category { get; set; } // Auth, Customer, Background, Internal

        public bool Enabled { get; set; } = true;

        // Routing Configuration
        public int? SenderIdentityId { get; set; }
        
        [ForeignKey("SenderIdentityId")]
        public virtual SenderIdentity? SenderIdentity { get; set; }

        public string? TemplateKey { get; set; } // FK to EmailTemplate logic (not strict DB FK to allow loose coupling if needed, but usually better as loose for templates)
        
        // Optional: Manual ReplyTo override just for this feature
        public string? ReplyToOverride { get; set; }

        public string? Notes { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedByUserId { get; set; }
    }
}
