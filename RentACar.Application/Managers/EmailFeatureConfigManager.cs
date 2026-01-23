using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Infrastructure.Data;

namespace RentACar.Application.Managers
{
    public class EmailFeatureConfigManager
    {
        private readonly ApplicationDbContext _context;
        private readonly SenderIdentityManager _senderManager; // Optional helper

        public EmailFeatureConfigManager(ApplicationDbContext context, SenderIdentityManager senderManager)
        {
            _context = context;
            _senderManager = senderManager;
        }

        public async Task<List<EmailFeatureConfig>> GetAllConfigsAsync()
        {
            // Seed if empty
            if (!await _context.EmailFeatureConfigs.AnyAsync())
            {
                await SeedConfigsAsync();
            }

            return await _context.EmailFeatureConfigs
                .Include(x => x.SenderIdentity)
                .OrderBy(x => x.Category)
                .ThenBy(x => x.FeatureName)
                .ToListAsync();
        }

        private async Task SeedConfigsAsync()
        {
            var seedData = new List<EmailFeatureConfig>
            {
                // Authentication
                new() { FeatureKey = "VerifyEmail", FeatureName = "Verify Email Address", Category = "Authentication" },
                new() { FeatureKey = "Otp2FA", FeatureName = "One-Time Password (OTP)", Category = "Authentication" },
                new() { FeatureKey = "ForgotPassword", FeatureName = "Forgot Password", Category = "Authentication" },
                new() { FeatureKey = "ResetPasswordFromSettings", FeatureName = "Reset Password (Settings)", Category = "Authentication" },
                new() { FeatureKey = "AccountStatusChanged", FeatureName = "Account Blocked/Unblocked", Category = "Authentication" },
                
                // Customer
                new() { FeatureKey = "BookingStatusChanged", FeatureName = "Booking Status Update", Category = "Customer" },
                new() { FeatureKey = "PaymentFailed", FeatureName = "Payment Failed Alert", Category = "Customer" },
                new() { FeatureKey = "PaymentInvoice", FeatureName = "Payment Invoice", Category = "Customer" },
                new() { FeatureKey = "DocumentStatusUpdate", FeatureName = "Document Status Update", Category = "Customer" },

                // Background
                new() { FeatureKey = "PaymentReminder", FeatureName = "Payment Due Reminder", Category = "Background" },
                new() { FeatureKey = "PickupReminder", FeatureName = "Pickup Instructions", Category = "Background" },
                new() { FeatureKey = "ReturnReminder", FeatureName = "Return Instructions", Category = "Background" },

                // Internal
                new() { FeatureKey = "PromoExpiryInternal", FeatureName = "Promo Expiring Alert", Category = "Internal" },
                new() { FeatureKey = "CarUpdatedInternal", FeatureName = "Fleet Car Updated", Category = "Internal" },
                new() { FeatureKey = "CategoryUpdatedInternal", FeatureName = "Category Pricing Updated", Category = "Internal" },
                new() { FeatureKey = "PromocodeUpdatedInternal", FeatureName = "Promocode Modified", Category = "Internal" },
                new() { FeatureKey = "UnverifiedDocsReminderInternal", FeatureName = "Unverified Docs Report", Category = "Internal" },
            };
            
            _context.EmailFeatureConfigs.AddRange(seedData);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateConfigAsync(EmailFeatureConfig config, string userId)
        {
            var existing = await _context.EmailFeatureConfigs.FirstOrDefaultAsync(x => x.FeatureKey == config.FeatureKey);
            if (existing != null)
            {
                existing.SenderIdentityId = config.SenderIdentityId;
                existing.TemplateKey = config.TemplateKey;
                existing.Enabled = config.Enabled;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedByUserId = userId;
                
                await _context.SaveChangesAsync();
            }
        }
    }
}
