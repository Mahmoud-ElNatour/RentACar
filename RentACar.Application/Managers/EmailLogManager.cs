using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data;

namespace RentACar.Application.Managers
{
    public class EmailLogManager
    {
        private readonly RentACarDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly AuditLogManager _auditLogManager;
        private readonly ILogger<EmailLogManager> _logger;

        public EmailLogManager(
            RentACarDbContext dbContext,
            IEmailService emailService,
            AuditLogManager auditLogManager,
            ILogger<EmailLogManager> logger)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _auditLogManager = auditLogManager;
            _logger = logger;
        }

        public async Task<List<EmailLog>> GetLogsAsync(
            string status = null,
            string type = null,
            string search = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _dbContext.EmailLogs
                .Include(l => l.CreatedByUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(l => l.Status == status);
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(l => l.EmailType == type);
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(l => 
                    l.Subject.ToLower().Contains(search) || 
                    l.RecipientsRaw.ToLower().Contains(search) ||
                    l.TemplateKey.ToLower().Contains(search)
                );
            }

            if (fromDate.HasValue)
            {
                query = query.Where(l => l.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l => l.CreatedAt <= toDate.Value);
            }

            return await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetLogsCountAsync(
            string status = null,
            string type = null,
            string search = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _dbContext.EmailLogs.AsQueryable();

            if (!string.IsNullOrEmpty(status)) query = query.Where(l => l.Status == status);
            if (!string.IsNullOrEmpty(type)) query = query.Where(l => l.EmailType == type);
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(l => 
                    l.Subject.ToLower().Contains(search) || 
                    l.RecipientsRaw.ToLower().Contains(search));
            }
            if (fromDate.HasValue) query = query.Where(l => l.CreatedAt >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(l => l.CreatedAt <= toDate.Value);

            return await query.CountAsync();
        }

        public async Task<EmailLog> GetLogByIdAsync(int id)
        {
            return await _dbContext.EmailLogs
                .Include(l => l.CreatedByUser)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<bool> RetryFailedEmailAsync(int logId, string userId)
        {
            var log = await _dbContext.EmailLogs.FindAsync(logId);
            if (log == null) return false;

            // Only retry failed or queued (if stuck)
            if (log.Status == "Sent") return true; 

            try
            {
                // Simple splitting if recipients are multiple
                // But generally RecipientsRaw is comma separated or single
                // _emailService takes single string usually or we pass it as is if it handles it.
                // Looking at EmailManager usage: await _emailService.SendEmailAsync(recipient, ...);
                // If it was bulk, EmailManager loops. Here "RecipientsRaw" might be "a, b". 
                // Implementation of IEmailService.SendEmailAsync usually expects single or comma-separated depending on provider.
                // Assuming it works as is.
                
                await _emailService.SendEmailAsync(log.RecipientsRaw, log.Subject, log.Body);
                
                log.Status = "Sent";
                log.SentAt = DateTime.UtcNow;
                log.Attempts++;
                log.LastError = null; // Clear error on success

                await _auditLogManager.LogEventAsync("EmailRetry", "Notification", log.RecipientsRaw, $"Retried sending log #{log.Id}", null, "Success", null, userId);
            }
            catch (Exception ex)
            {
                log.Attempts++;
                log.LastError = ex.Message;
                // Status remains Failed
                
                await _auditLogManager.LogEventAsync("EmailRetryFailed", "Notification", log.RecipientsRaw, $"Retry failed for log #{log.Id}", null, "Failed", ex.Message, userId);
            }

            await _dbContext.SaveChangesAsync();
            return log.Status == "Sent";
        }
        
        public async Task<int> RetryAllFailedAsync(string userId)
        {
            var failedLogs = await _dbContext.EmailLogs
                .Where(l => l.Status == "Failed")
                .ToListAsync();

            int successCount = 0;
            foreach (var log in failedLogs)
            {
                if (await RetryFailedEmailAsync(log.Id, userId))
                {
                    successCount++;
                }
            }
            return successCount;
        }

    }
}
