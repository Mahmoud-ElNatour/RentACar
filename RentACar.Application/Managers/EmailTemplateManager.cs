using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Infrastructure.Data;

namespace RentACar.Application.Managers
{
    public class EmailTemplateManager
    {
        private readonly RentACarDbContext _context;

        public EmailTemplateManager(RentACarDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmailTemplate>> GetAllTemplatesAsync()
        {
            return await _context.EmailTemplates
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<EmailTemplate> GetTemplateByIdAsync(int id)
        {
            return await _context.EmailTemplates.FindAsync(id);
        }

        public async Task<EmailTemplate> GetTemplateByKeyAsync(string key)
        {
            return await _context.EmailTemplates
                .FirstOrDefaultAsync(t => t.TemplateKey == key);
        }

        public async Task<int> CreateTemplateAsync(EmailTemplate template, string userId)
        {
            // Ensure unique key
            if (await _context.EmailTemplates.AnyAsync(t => t.TemplateKey == template.TemplateKey))
                throw new Exception($"Template Key '{template.TemplateKey}' already exists.");

            template.UpdatedAt = DateTime.UtcNow;
            template.UpdatedByUserId = userId;
            
            _context.EmailTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template.Id;
        }

        public async Task UpdateTemplateAsync(EmailTemplate template, string userId)
        {
            var existing = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.TemplateKey == template.TemplateKey);
            if (existing == null) throw new Exception("Template not found");

            existing.Name = template.Name;
            existing.Subject = template.Subject;
            existing.Body = template.Body;
            existing.Category = template.Category;
            existing.IsActive = template.IsActive; // Support status update
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = userId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteTemplateAsync(int id)
        {
            var t = await _context.EmailTemplates.FindAsync(id);
            if (t != null)
            {
                _context.EmailTemplates.Remove(t);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task ToggleActiveAsync(int id, string userId)
        {
            var t = await _context.EmailTemplates.FindAsync(id);
            if (t != null)
            {
                t.IsActive = !t.IsActive;
                t.UpdatedAt = DateTime.UtcNow;
                t.UpdatedByUserId = userId;
                await _context.SaveChangesAsync();
            }
        }
        public async Task DeleteTemplateByKeyAsync(string key)
        {
            var t = await _context.EmailTemplates.FirstOrDefaultAsync(x => x.TemplateKey == key);
            if (t != null)
            {
                _context.EmailTemplates.Remove(t);
                await _context.SaveChangesAsync();
            }
        }
    }
}
