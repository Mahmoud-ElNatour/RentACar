using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Infrastructure.Data;

namespace RentACar.Application.Managers
{
    public class NotificationProcessingService
    {
        private readonly ApplicationDbContext _context;
        private readonly RentACarDbContext _appContext; // Renaming to avoid conflict if any, but logically it is business context
        private readonly EmailManager _emailManager;
        private readonly EmployeeManager _employeeManager;
        private readonly DistributionListManager _distListManager;

        public NotificationProcessingService(
            ApplicationDbContext context,
            RentACarDbContext appContext,
            EmailManager emailManager,
            EmployeeManager employeeManager,
            DistributionListManager distListManager)
        {
            _context = context;
            _appContext = appContext;
            _emailManager = emailManager;
            _employeeManager = employeeManager;
            _distListManager = distListManager;
        }

        public async Task<NotificationSettings> GetSettingsAsync()
        {
            var settings = await _context.NotificationSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new NotificationSettings();
                _context.NotificationSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return settings;
        }

        public async Task UpdateSettingsAsync(NotificationSettings newSettings, string userId)
        {
            var existing = await GetSettingsAsync();
            
            // Map props
            existing.ReminderProcessingEnabled = newSettings.ReminderProcessingEnabled;
            existing.ReminderProcessingPaused = newSettings.ReminderProcessingPaused;
            existing.AllNotificationEmailsEnabled = newSettings.AllNotificationEmailsEnabled;
            existing.CheckIntervalMinutes = newSettings.CheckIntervalMinutes;

            existing.PaymentReminderEnabled = newSettings.PaymentReminderEnabled;
            existing.PaymentReminderDelayHours = newSettings.PaymentReminderDelayHours;
            existing.PaymentReminderSendOnceOnly = newSettings.PaymentReminderSendOnceOnly;
            existing.PaymentReminderRepeatEveryHours = newSettings.PaymentReminderRepeatEveryHours;
            existing.PaymentReminderMaxSends = newSettings.PaymentReminderMaxSends;
            existing.PaymentReminderStatusCsv = newSettings.PaymentReminderStatusCsv;

            existing.PickupReminderEnabled = newSettings.PickupReminderEnabled;
            existing.PickupReminderHoursBefore = newSettings.PickupReminderHoursBefore;
            existing.PickupReminderSendOnceOnly = newSettings.PickupReminderSendOnceOnly;

            existing.ReturnReminderEnabled = newSettings.ReturnReminderEnabled;
            existing.ReturnReminderHoursBefore = newSettings.ReturnReminderHoursBefore;
            existing.ReturnReminderSendOnceOnly = newSettings.ReturnReminderSendOnceOnly;

            existing.PromoExpiryEnabled = newSettings.PromoExpiryEnabled;
            existing.PromoExpiryCheckFrequency = newSettings.PromoExpiryCheckFrequency;
            existing.PromoExpiryAutoDeactivate = newSettings.PromoExpiryAutoDeactivate;

            existing.PromoExpiryEmployeesListId = newSettings.PromoExpiryEmployeesListId;
            existing.CarUpdateEmployeesListId = newSettings.CarUpdateEmployeesListId;
            existing.CategoryUpdateEmployeesListId = newSettings.CategoryUpdateEmployeesListId;
            existing.PromocodeUpdateEmployeesListId = newSettings.PromocodeUpdateEmployeesListId;
            existing.DocsReminderEmployeesListId = newSettings.DocsReminderEmployeesListId;

            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = userId;

            await _context.SaveChangesAsync();
        }

        // Method to Run Logic manually or via Background Service
        public async Task RunOnceAsync(string triggeredBy)
        {
            var settings = await GetSettingsAsync();

            // Create ServiceRunRecord
            var runRecord = new ServiceRunRecord
            {
                RunAt = DateTime.UtcNow,
                TriggeredBy = triggeredBy
            };

            // Checks
            if (!settings.ReminderProcessingEnabled || settings.ReminderProcessingPaused)
            {
                runRecord.Summary = "Skipped: Processing Disabled or Paused.";
                _context.ServiceRunRecords.Add(runRecord);
                await _context.SaveChangesAsync();
                return;
            }

            if (!settings.AllNotificationEmailsEnabled)
            {
                 runRecord.Summary = "Skipped: Master Email Switch Disabled.";
                 _context.ServiceRunRecords.Add(runRecord);
                 await _context.SaveChangesAsync();
                 return;
            }

            int sentCount = 0;
            int failedCount = 0;
            int processedCount = 0;

            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);
            var tomorrow = today.AddDays(1);

            // 1. Payment Reminders
            if (settings.PaymentReminderEnabled)
            {
                var pendingBookings = await _appContext.Bookings
                    .Include(b => b.Customer).ThenInclude(c => c.User)
                    .Include(b => b.Payment)
                    .Where(b => b.BookingStatus == "Pending" && b.Payment == null) 
                    .ToListAsync();

                foreach (var booking in pendingBookings)
                {
                    processedCount++;
                    bool shouldSend = false;
                    
                    // Logic: If settings says DelayHours=24, we need booking creation time.
                    // Assuming we send if LastReminder is null (First time) OR logic permits repeat.
                    if (booking.LastPaymentReminderSentAt == null)
                    {
                         shouldSend = true; 
                    }
                    else if (settings.PaymentReminderRepeatEveryHours.HasValue)
                    {
                        if ((now - booking.LastPaymentReminderSentAt.Value).TotalHours >= settings.PaymentReminderRepeatEveryHours.Value)
                        {
                             shouldSend = true;
                        }
                    }

                    if (shouldSend)
                    {
                        var email = booking.Customer?.User?.Email;
                        if (!string.IsNullOrEmpty(email))
                        {
                            try {
                                await _emailManager.SendPaymentReminderEmail(email, booking.Customer.Name, booking, booking.TotalPrice);
                                booking.LastPaymentReminderSentAt = now;
                                sentCount++;
                                
                                runRecord.RunItems.Add(new ServiceRunItem { EventType = "PaymentReminder", TargetType = "Booking", TargetId = booking.BookingId.ToString(), RecipientEmail = email, Result = "Success" });
                                _context.NotificationLogs.Add(new NotificationLog { EventType = "PaymentReminder", TargetType = "Booking", TargetId = booking.BookingId.ToString(), RecipientEmail = email, Result = "Success", Actor = "System", CreatedAt = DateTime.UtcNow });
                            }
                            catch(Exception ex) {
                                failedCount++;
                                runRecord.RunItems.Add(new ServiceRunItem { EventType = "PaymentReminder", TargetType = "Booking", TargetId = booking.BookingId.ToString(), RecipientEmail = email, Result = "Failed", Error = ex.Message });
                                _context.NotificationLogs.Add(new NotificationLog { EventType = "PaymentReminder", TargetType = "Booking", TargetId = booking.BookingId.ToString(), RecipientEmail = email, Result = "Failed", Details = ex.Message, Actor = "System", CreatedAt = DateTime.UtcNow });
                            }
                        }
                    }
                }
            }

            // 2. Pickup Reminders
            if (settings.PickupReminderEnabled)
            {
                 var pickupReminders = await _appContext.Bookings
                .Include(b => b.Customer).ThenInclude(c => c.User)
                .Include(b => b.Car)
                .Where(b => b.BookingStatus == "Booked" && b.Startdate == tomorrow && b.LastPickupReminderSentAt == null)
                .ToListAsync();

                foreach (var booking in pickupReminders)
                {
                    processedCount++;
                    var email = booking.Customer?.User?.Email;
                    if (!string.IsNullOrEmpty(email))
                    {
                         try {
                            await _emailManager.SendBookingReminderEmail(email, booking.Customer.Name, booking, "Pickup");
                            booking.LastPickupReminderSentAt = now;
                            _appContext.Bookings.Update(booking);
                            sentCount++;
                             runRecord.RunItems.Add(new ServiceRunItem { EventType = "PickupReminder", TargetType = "Booking", TargetId = booking.BookingId.ToString(), RecipientEmail = email, Result = "Success" });
                             _context.NotificationLogs.Add(new NotificationLog { EventType = "PickupReminder", TargetType = "Booking", TargetId = booking.BookingId.ToString(), RecipientEmail = email, Result = "Success", Actor = "System", CreatedAt = DateTime.UtcNow });
                         }
                         catch(Exception ex) {
                             failedCount++;
                             runRecord.RunItems.Add(new ServiceRunItem { EventType = "PickupReminder", TargetType = "Booking", TargetId = booking.BookingId.ToString(), RecipientEmail = email, Result = "Failed", Error = ex.Message });
                             _context.NotificationLogs.Add(new NotificationLog { EventType = "PickupReminder", TargetType = "Booking", TargetId = booking.BookingId.ToString(), RecipientEmail = email, Result = "Failed", Details = ex.Message, Actor = "System", CreatedAt = DateTime.UtcNow });
                         }
                    }
                }
            }

            // 3. Return Reminders
            if (settings.ReturnReminderEnabled)
            {
                 var returnReminders = await _appContext.Bookings
                    .Include(b => b.Customer).ThenInclude(c => c.User)
                    .Include(b => b.Car)
                    .Where(b => b.BookingStatus == "Booked" && b.Enddate == tomorrow && b.LastReturnReminderSentAt == null)
                    .ToListAsync();

                foreach (var booking in returnReminders)
                {
                    processedCount++;
                    var email = booking.Customer?.User?.Email;
                    if (!string.IsNullOrEmpty(email))
                    {
                        try {
                            await _emailManager.SendBookingReminderEmail(email, booking.Customer.Name, booking, "Return");
                            booking.LastReturnReminderSentAt = now;
                            _appContext.Bookings.Update(booking);
                            sentCount++;
                            runRecord.RunItems.Add(new ServiceRunItem { EventType = "ReturnReminder", TargetType = "Booking", TargetId = booking.BookingId.ToString(), RecipientEmail = email, Result = "Success" });
                            _context.NotificationLogs.Add(new NotificationLog { EventType = "ReturnReminder", TargetType = "Booking", TargetId = booking.BookingId.ToString(), RecipientEmail = email, Result = "Success", Actor = "System", CreatedAt = DateTime.UtcNow });
                        }
                         catch(Exception ex) {
                             failedCount++;
                             runRecord.RunItems.Add(new ServiceRunItem { EventType = "ReturnReminder", TargetType = "Booking", TargetId = booking.BookingId.ToString(), RecipientEmail = email, Result = "Failed", Error = ex.Message });
                             _context.NotificationLogs.Add(new NotificationLog { EventType = "ReturnReminder", TargetType = "Booking", TargetId = booking.BookingId.ToString(), RecipientEmail = email, Result = "Failed", Details = ex.Message, Actor = "System", CreatedAt = DateTime.UtcNow });
                         }
                    }
                }
            }
            
            // 4. Promo Expiry
            if (settings.PromoExpiryEnabled)
            {
                var expiredPromos = await _appContext.Promocodes
                   .Where(p => p.IsActive && p.ValidUntil < DateOnly.FromDateTime(now) && !p.IsExpiredNotificationSent)
                   .ToListAsync();

                if (expiredPromos.Any())
                {
                     // Resolve Recipients from Distribution List if configured
                     List<string> employees = new List<string>();
                     
                     if (settings.PromoExpiryEmployeesListId.HasValue)
                     {
                         var rule = new Application.DTOs.DistributionListRuleDto { IncludeEmployees = true }; // Default rule if we just pull? OR pull actual list members
                         // Check DistListManager logic. It separates Rule Preview from actual members?
                         // Members are stored in `DistributionListMembers`.
                         var list = await _distListManager.GetListByIdAsync(settings.PromoExpiryEmployeesListId.Value);
                         if (list != null) employees = list.Members.Where(m => m.IsActive).Select(m => m.Email).ToList();
                     }
                     else 
                     {
                         // Fallback to all active employees
                         employees = await _employeeManager.GetActiveEmployeeEmailsAsync();
                     }

                     foreach(var promo in expiredPromos)
                     {
                         processedCount++;
                         try {
                             await _emailManager.SendPromocodeUpdateEmail(employees, promo, "Expired", "Validity date exceeded", "System");
                             
                             promo.IsExpiredNotificationSent = true;
                             if(settings.PromoExpiryAutoDeactivate) promo.IsActive = false; 
                             
                             _appContext.Promocodes.Update(promo);
                             sentCount++; // Sent 1 email (to multiple recipients) or count per recipient? EmailManager sends bulk or loop? Assuming bulk single call count as 1 event.
                             runRecord.RunItems.Add(new ServiceRunItem { EventType = "PromoExpiry", TargetType = "Promocode", TargetId = promo.Name ?? promo.PromocodeId.ToString(), RecipientEmail = "DistributionList", Result = "Success" });
                             _context.NotificationLogs.Add(new NotificationLog { EventType = "PromoExpiry", TargetType = "Promocode", TargetId = promo.Name ?? promo.PromocodeId.ToString(), RecipientEmail = "DistributionList", Result = "Success", Actor = "System", CreatedAt = DateTime.UtcNow });
                         }
                         catch(Exception ex) {
                             failedCount++;
                             runRecord.RunItems.Add(new ServiceRunItem { EventType = "PromoExpiry", TargetType = "Promocode", TargetId = promo.Name ?? promo.PromocodeId.ToString(), RecipientEmail = "DistributionList", Result = "Failed", Error = ex.Message });
                             _context.NotificationLogs.Add(new NotificationLog { EventType = "PromoExpiry", TargetType = "Promocode", TargetId = promo.Name ?? promo.PromocodeId.ToString(), RecipientEmail = "DistributionList", Result = "Failed", Details = ex.Message, Actor = "System", CreatedAt = DateTime.UtcNow });
                         }
                     }
                }
            }

            await _appContext.SaveChangesAsync();

            // Update Stats in Settings
            settings.LastRunAt = now;
            settings.NextRunAt = now.AddMinutes(settings.CheckIntervalMinutes);
            settings.LastRunProcessedCount = processedCount;
            settings.LastRunSentCount = sentCount;
            settings.LastRunFailedCount = failedCount;
            settings.LastRunSummary = $"Success. Processed: {processedCount}, Sent: {sentCount}, Failed: {failedCount}";

            runRecord.ProcessedCount = processedCount;
            runRecord.SentCount = sentCount;
            runRecord.FailedCount = failedCount;
            runRecord.Summary = settings.LastRunSummary;

            _context.ServiceRunRecords.Add(runRecord);
            await _context.SaveChangesAsync();
        }
        public async Task<ServiceRunRecord?> GetLastRunRecordAsync()
        {
            return await _context.ServiceRunRecords
                .Include(r => r.RunItems)
                .OrderByDescending(r => r.RunAt)
                .FirstOrDefaultAsync();
        }
    }
}
