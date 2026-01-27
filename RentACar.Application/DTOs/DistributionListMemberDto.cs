using System;

namespace RentACar.Application.DTOs
{
    public class DistributionListMemberDto
    {
        public int Id { get; set; }
        public int DistributionListId { get; set; }
        public string Email { get; set; }
        public string Label { get; set; }
        public string MemberType { get; set; } // Employee, Admin, Customer, Other
        public bool IsActive { get; set; }
        public DateTime AddedAt { get; set; }
        public string AddedByUserId { get; set; }
    }
}
