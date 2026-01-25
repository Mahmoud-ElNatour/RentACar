using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities
{
    public class DistributionListRule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DistributionListId { get; set; }

        public bool IncludeEmployees { get; set; }
        public bool IncludeAdmins { get; set; }
        public bool IncludeCustomers { get; set; }
        
        public bool OnlyActiveUsers { get; set; }
        public bool ExcludeBlacklistedCustomers { get; set; }
        public bool OnlyVerifiedEmails { get; set; }

        // Comma/Newline separated emails
        public string ManualEmailsRaw { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("DistributionListId")]
        public virtual DistributionList DistributionList { get; set; }
    }
}
