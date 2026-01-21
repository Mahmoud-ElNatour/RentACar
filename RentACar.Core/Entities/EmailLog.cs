using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Core.Entities
{
    public class EmailLog
    {
        [Key]
        public int Id { get; set; }

        // Manual, Template, Reminder
        public string EmailType { get; set; }
        public string TemplateKey { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }

        public string RecipientsRaw { get; set; }

        // Queued, Sent, Failed
        public string Status { get; set; }

        public int Attempts { get; set; }
        public string LastError { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }

        public string CreatedByUserId { get; set; }
    }
}
