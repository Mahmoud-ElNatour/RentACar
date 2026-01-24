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
            return await _context.EmailFeatureConfigs
                .Include(x => x.SenderIdentity)
                .OrderBy(x => x.Category)
                .ThenBy(x => x.FeatureName)
                .ToListAsync();
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
