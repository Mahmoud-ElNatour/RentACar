using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RentACar.Core.Constants;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Application.Managers
{
    public class EmailTemplateManager
    {
        private readonly IEmailTemplateRepository _emailTemplateRepository;

        public EmailTemplateManager(IEmailTemplateRepository emailTemplateRepository)
        {
            _emailTemplateRepository = emailTemplateRepository;
        }

        public async Task<List<EmailTemplate>> GetAllTemplatesAsync()
        {
            var templates = await _emailTemplateRepository.GetAllAsync();
            return templates.OrderBy(t => t.Category).ThenBy(t => t.Name).ToList();
        }

        public async Task<EmailTemplate> GetTemplateByKeyAsync(string key)
        {
            return await _emailTemplateRepository.GetByKeyAsync(key);
        }

        public async Task<EmailTemplate> GetTemplateByIdAsync(int id)
        {
            return await _emailTemplateRepository.GetByIdAsync(id);
        }

        public async Task UpdateTemplateAsync(EmailTemplate template)
        {
            await _emailTemplateRepository.UpdateAsync(template);
        }

        public async Task CreateTemplateAsync(EmailTemplate template)
        {
            // Ensure ID is 0 so EF knows it's new, though usually it is by default
            template.Id = 0;
            template.UpdatedAt = DateTime.UtcNow;
            
            // Check if key exists? For now assume controller handles or unique constraint throws
            await _emailTemplateRepository.AddAsync(template);
        }

        public async Task ResetTemplateToDefaultAsync(string key)
        {
            var template = await GetTemplateByKeyAsync(key);
            if (template == null) return;

            // Define defaults
            string defaultSubject = "";
            string defaultBody = "";

            switch (key)
            {
                case EmailTemplateKeys.BookingConfirmation:
                    defaultSubject = "Your Reservation #{{BookingRef}} is Confirmed";
                    defaultBody = "<h1>Confirmed</h1><p>Dear {{CustomerName}},</p><p>Your reservation {{BookingRef}} is confirmed.</p>";
                    break;
                case EmailTemplateKeys.PaymentReminder:
                    defaultSubject = "Payment Reminder: Invoice #{{InvoiceId}}";
                    defaultBody = "<p>Dear {{CustomerName}},</p><p>Please pay your invoice.</p>";
                    break;
                 // Add cases for other keys as needed
                default:
                    defaultSubject = "New Notification";
                    defaultBody = "<p>Notification content.</p>";
                    break;
            }

            template.Subject = defaultSubject;
            template.Body = defaultBody;
            template.UpdatedAt = DateTime.UtcNow;
            
            await UpdateTemplateAsync(template);
        }

        public async Task DeleteTemplateAsync(string key)
        {
            var template = await _emailTemplateRepository.GetByKeyAsync(key);
            if (template != null)
            {
                await _emailTemplateRepository.DeleteAsync(template);
            }
        }
    }
}
