using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RentACar.Core.Enums;

namespace RentACar.Core.Entities
{
    public class AiMessage
    {
        [Key]
        public int AiMessageId { get; set; }

        [Required]
        public int AiConversationId { get; set; }

        [Required]
        public AiSenderType Sender { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("AiConversationId")]
        public virtual AiConversation Conversation { get; set; }
    }
}
