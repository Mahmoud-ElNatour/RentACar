using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class DistributionListDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedByUserId { get; set; }
        public string UpdatedByUserId { get; set; }

        public int MemberCount { get; set; } // For display

        public List<DistributionListMemberDto> Members { get; set; } = new List<DistributionListMemberDto>();
        public List<DistributionListRuleDto> Rules { get; set; } = new List<DistributionListRuleDto>();
    }
}
