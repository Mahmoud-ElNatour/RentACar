using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Core.Entities
{
    public class EmailDraft
    {
        [Key]
        public int Id { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }

        // Manual recipients
        public string RecipientsRaw { get; set; }

        // Selected distribution list IDs (CSV or JSON)
        public string SelectedDistributionListIdsRaw { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public string CreatedByUserId { get; set; }
    }
}
