using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities
{
    public class AiConversation
    {
        [Key]
        public int AiConversationId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

        public bool IsEscalated { get; set; } = false;

        // Navigation Properties
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public virtual ICollection<AiMessage> Messages { get; set; } = new List<AiMessage>();
    }
}
