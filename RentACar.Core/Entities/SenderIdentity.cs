using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities
{
    public class SenderIdentity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string FromEmail { get; set; }

        [EmailAddress]
        public string? ReplyToEmail { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;

        public string? VerifiedStatus { get; set; } // Verified, Pending, Unknown

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public string? CreatedByUserId { get; set; }
        public string? UpdatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual AspNetUser? CreatedByUser { get; set; }

        [ForeignKey("UpdatedByUserId")]
        public virtual AspNetUser? UpdatedByUser { get; set; }
    }
}
