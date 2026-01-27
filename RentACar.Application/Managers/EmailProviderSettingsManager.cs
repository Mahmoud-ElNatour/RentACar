using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Infrastructure.Data;

namespace RentACar.Application.Managers
{
    public class EmailProviderSettingsManager
    {
        private readonly ApplicationDbContext _context;

        public EmailProviderSettingsManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmailProviderSettings> GetSettingsAsync()
        {
            var settings = await _context.EmailProviderSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new EmailProviderSettings();
                _context.EmailProviderSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return settings;
        }

        public async Task UpdateSettingsAsync(EmailProviderSettings settings, string userId)
        {
            var existing = await GetSettingsAsync();

            existing.ProviderType = settings.ProviderType;
            // existing.MailjetApiKey = settings.MailjetApiKey; // Removed from DB
            // existing.MailjetSecretKey = settings.MailjetSecretKey; // Removed from DB

            existing.SenderDomain = settings.SenderDomain;
            existing.DefaultReplyToEmail = settings.DefaultReplyToEmail;
            existing.SandboxModeEnabled = settings.SandboxModeEnabled;
            existing.RateLimitPerMinute = settings.RateLimitPerMinute;
            existing.RetryCount = settings.RetryCount;
            existing.RetryDelayMinutes = settings.RetryDelayMinutes;
            
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = userId;

            await _context.SaveChangesAsync();
        }
    }
}
