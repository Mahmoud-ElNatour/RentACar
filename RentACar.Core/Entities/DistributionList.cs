using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities
{
    public class DistributionList
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public virtual AspNetUser? CreatedByUser { get; set; }

        public string? UpdatedByUserId { get; set; }
        [ForeignKey("UpdatedByUserId")]
        public virtual AspNetUser? UpdatedByUser { get; set; }

        public virtual ICollection<DistributionListMember> Members { get; set; } = new List<DistributionListMember>();
        public virtual ICollection<DistributionListRule> Rules { get; set; } = new List<DistributionListRule>();
    }
}
