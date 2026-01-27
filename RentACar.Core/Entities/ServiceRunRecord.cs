using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace RentACar.Core.Entities
{
    public class ServiceRunRecord
    {
        [Key]
        public int Id { get; set; }

        public DateTime RunAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public string TriggeredBy { get; set; } // "System" or "Admin:UserId"

        public int ProcessedCount { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }

        public string? Summary { get; set; }

        public virtual ICollection<ServiceRunItem> RunItems { get; set; } = new List<ServiceRunItem>();
    }
}
