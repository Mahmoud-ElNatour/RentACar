using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities
{
    public class SupportMessage
    {
        [Key]
        public int SupportMessageId { get; set; }

        [Required]
        public int SupportConversationId { get; set; }

        [Required]
        [StringLength(450)]
        public string SenderUserId { get; set; }

        [Required]
        [StringLength(50)]
        public string SenderRole { get; set; }

        [Required]
        public string MessageText { get; set; }

        public string? AttachmentUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsInternalNote { get; set; } = false;

        // Navigation Properties
        [ForeignKey("SupportConversationId")]
        public virtual SupportConversation Conversation { get; set; }

        [ForeignKey("SenderUserId")]
        public virtual AspNetUser Sender { get; set; }
    }
}
