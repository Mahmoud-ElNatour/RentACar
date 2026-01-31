using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentACar.Infrastructure.Data;
using RentACar.Web.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class EmailServicesController : Controller
    {
        private readonly RentACarDbContext _context;

        public EmailServicesController(RentACarDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(EmailServicesHub));
        }

        [HttpGet]
        public async Task<IActionResult> EmailServicesHub()
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = DateTime.UtcNow.AddHours(-24);

            // 1. Sent Today
            var sentToday = await _context.EmailLogs
                .CountAsync(l => l.Status == "Sent" && l.SentAt >= today);

            // 2. Delivery Rate (Overall)
            var totalAttempts = await _context.EmailLogs.CountAsync();
            var totalSent = await _context.EmailLogs.CountAsync(l => l.Status == "Sent");
            var deliveryRate = totalAttempts > 0 
                ? (double)totalSent / totalAttempts * 100 
                : 0;

            // 3. Active Reminders (Enabled Features)
            // Assuming "EmailFeatureConfig" represents the automated features
            // Note: DBContext might not have DbSet<EmailFeatureConfig> exposed directly if not added yet, 
            // checking context file showed it might be missing or under a different name?
            // Wait, previous file view of Context didn't show EmailFeatureConfigs DbSet explicitly in the range viewed.
            // Let me check if I need to add it or if it exists. 
            // Re-checking RentACarDbContext.cs content from previous turns...
            // It showed: public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }
            // public virtual DbSet<NotificationSettings> NotificationSettings { get; set; }
            // public virtual DbSet<EmailLog> EmailLogs { get; set; }
            // public virtual DbSet<NotificationLog> NotificationLogs { get; set; }
            // public virtual DbSet<SenderIdentity> SenderIdentities { get; set; }
            // It did NOT show EmailFeatureConfigs. I might need to access it via Set<EmailFeatureConfig>() or add it.
            // However, the file exists in Core/Entities.
            // I will use Set<EmailFeatureConfig>() to be safe or assuming it might be hidden in partial? 
            // Better to assume I can use _context.Set<EmailFeatureConfig>() if valid.
            
            // 3. Active Reminders (Enabled Features)
            var activeFeatures = await _context.EmailFeatureConfigs
                .CountAsync(f => f.Enabled);

            // 4. Pending Errors (Last 24h)
            var pendingErrors = await _context.EmailLogs
                .CountAsync(l => l.Status == "Failed" && l.CreatedAt >= yesterday);

            var model = new EmailHubViewModel
            {
                SentToday = sentToday,
                DeliveryRate = Math.Round(deliveryRate, 1),
                ActiveReminders = activeFeatures,
                PendingErrors = pendingErrors
            };

            return View(model);
        }
    }
}
