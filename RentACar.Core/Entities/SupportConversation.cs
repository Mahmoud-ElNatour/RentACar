using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities
{
    public class SupportConversation
    {
        [Key]
        public int SupportConversationId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public int? BookingId { get; set; }

        [Required]
        [StringLength(150)]
        public string Subject { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public int? AssignedEmployeeId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        public bool RequiresHumanIntervention { get; set; } = false;

        // Navigation Properties
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }

        [ForeignKey("AssignedEmployeeId")]
        public virtual Employee? AssignedEmployee { get; set; }

        public virtual ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
    }
}
