using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities
{
    public class DistributionListMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DistributionListId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        public string Label { get; set; }

        // Employee, Admin, Customer, Other
        public string MemberType { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public string AddedByUserId { get; set; }

        [ForeignKey("DistributionListId")]
        public virtual DistributionList DistributionList { get; set; }
    }
}
