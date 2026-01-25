using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Core.Entities
{
    public class NotificationLog
    {
        [Key]
        public int Id { get; set; }

        // PaymentReminder, PickupReminder, ReturnReminder, PromoExpiry
        public string EventType { get; set; }

        // Booking, Promocode
        public string TargetType { get; set; }
        public string TargetId { get; set; }

        public string RecipientEmail { get; set; }

        // Success, Failed, Skipped
        public string Result { get; set; }
        public string Details { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // System, AdminManualRun
        public string Actor { get; set; }
    }
}
