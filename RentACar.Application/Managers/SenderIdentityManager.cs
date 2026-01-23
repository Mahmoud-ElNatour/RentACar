using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Infrastructure.Data;

namespace RentACar.Application.Managers
{
    public class SenderIdentityManager
    {
        private readonly ApplicationDbContext _context;

        public SenderIdentityManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SenderIdentity>> GetAllIdentitiesAsync()
        {
            return await _context.SenderIdentities.ToListAsync();
        }

        public async Task<SenderIdentity?> GetIdentityByIdAsync(int id)
        {
            return await _context.SenderIdentities.FindAsync(id);
        }

        public async Task CreateIdentityAsync(SenderIdentity identity, string userId)
        {
            if (identity.IsDefault)
            {
                // Unset other defaults
                var defaults = await _context.SenderIdentities.Where(s => s.IsDefault).ToListAsync();
                foreach (var d in defaults) d.IsDefault = false;
            }

            identity.CreatedAt = DateTime.UtcNow;
            identity.CreatedByUserId = userId;
            
            _context.SenderIdentities.Add(identity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateIdentityAsync(SenderIdentity identity, string userId)
        {
            var existing = await _context.SenderIdentities.FindAsync(identity.Id);
            if (existing == null) throw new Exception("Identity not found");

            existing.DisplayName = identity.DisplayName;
            existing.FromEmail = identity.FromEmail;
            existing.ReplyToEmail = identity.ReplyToEmail;
            
            if (identity.IsDefault && !existing.IsDefault)
            {
                // Unset others
                var defaults = await _context.SenderIdentities.Where(s => s.IsDefault).ToListAsync();
                foreach (var d in defaults) d.IsDefault = false;
            }
            existing.IsDefault = identity.IsDefault;
            existing.IsActive = identity.IsActive;

            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = userId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteIdentityAsync(int id)
        {
            var existing = await _context.SenderIdentities.FindAsync(id);
            if (existing != null)
            {
                _context.SenderIdentities.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task ToggleActiveAsync(int id, string userId)
        {
             var existing = await _context.SenderIdentities.FindAsync(id);
             if (existing != null)
             {
                 existing.IsActive = !existing.IsActive;
                 existing.UpdatedAt = DateTime.UtcNow;
                 existing.UpdatedByUserId = userId;
                 await _context.SaveChangesAsync();
             }
        }
    }
}
