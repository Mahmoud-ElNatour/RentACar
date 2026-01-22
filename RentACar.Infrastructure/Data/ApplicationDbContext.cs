using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;

namespace RentACar.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<DistributionList> DistributionLists { get; set; }
        public DbSet<DistributionListMember> DistributionListMembers { get; set; }
        public DbSet<DistributionListRule> DistributionListRules { get; set; }
        public DbSet<EmailDraft> EmailDrafts { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


    }
}
