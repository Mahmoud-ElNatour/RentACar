using System;

namespace RentACar.Application.DTOs
{
    public class DistributionListRuleDto
    {
        public int Id { get; set; }
        public int DistributionListId { get; set; }
        public bool IncludeEmployees { get; set; }
        public bool IncludeAdmins { get; set; }
        public bool IncludeCustomers { get; set; }
        public bool OnlyActiveUsers { get; set; }
        public bool ExcludeBlacklistedCustomers { get; set; }
        public bool OnlyVerifiedEmails { get; set; }
        public string ManualEmailsRaw { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
