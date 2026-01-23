using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities
{
    public class ServiceRunItem
    {
        [Key]
        public int Id { get; set; }

        public int ServiceRunRecordId { get; set; }
        [ForeignKey("ServiceRunRecordId")]
        public virtual ServiceRunRecord ServiceRunRecord { get; set; }

        public string EventType { get; set; } // PaymentReminder, PickupReminder...
        public string TargetType { get; set; } // Booking, Promocode
        public string TargetId { get; set; }   // ID of the entity processed

        public string RecipientEmail { get; set; }

        public string Result { get; set; } // Success, Failed, Skipped
        public string? Error { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
