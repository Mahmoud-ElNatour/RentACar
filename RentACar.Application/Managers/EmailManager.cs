using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data;
using RentACar.Core.Entities;

namespace RentACar.Application.Managers
{
    public class EmailManager
    {
        private readonly IEmailService _emailService;
        private readonly RentACarDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AuditLogManager _auditLogManager;

        public EmailManager(
            IEmailService emailService,
            RentACarDbContext dbContext,
            UserManager<IdentityUser> userManager,
            AuditLogManager auditLogManager)
        {
            _emailService = emailService;
            _dbContext = dbContext;
            _userManager = userManager;
            _auditLogManager = auditLogManager;
        }

        private async Task<bool> SendEmailSafeAsync(string recipient, string subject, string message, string emailType, Dictionary<string, byte[]> attachments = null, string userId = null)
        {
            if (string.IsNullOrEmpty(recipient)) return false;
            
            var emailLog = new EmailLog
            {
                RecipientsRaw = recipient,
                Subject = subject ?? "",
                Body = message ?? "",
                EmailType = emailType,
                CreatedAt = DateTime.UtcNow,
                Attempts = 1,
                CreatedByUserId = userId ?? "System",
                TemplateKey = "",
                LastError = "",
                Status = "Pending"
            };

            try
            {
                // Try to get current user ID relative to the request context if possible, or from a passed parameter?
                // For now, we leave Creator as null/System unless we refactor to pass userId.
                
                await _emailService.SendEmailAsync(recipient, subject, message, attachments);
                
                emailLog.Status = "Sent";
                emailLog.SentAt = DateTime.UtcNow;
                
                await _auditLogManager.LogEventAsync("EmailSent", "Notification", recipient, $"Sent {emailType} to {recipient}", status: "Success");
            }
            catch (Exception ex)
            {
                emailLog.Status = "Failed";
                emailLog.LastError = ex.Message;
                
                await _auditLogManager.LogEventAsync("EmailFailed", "Notification", recipient, $"Failed to send {emailType}", status: "Failed", failureReason: ex.Message);
            }
            
            // Save Email Log
            try 
            {
                _dbContext.EmailLogs.Add(emailLog);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Debugging: Throw to see the error in the response
                throw new InvalidOperationException($"Email Log Save Failed: {ex.Message} | Inner: {ex.InnerException?.Message}", ex);
            }

            return emailLog.Status == "Sent";
        }

        // 1. BOOKING STATUS UPDATE EMAILS
        public async Task SendBookingStatusEmail(string email, string customerName, Booking booking, string reason = null)
        {
            string subject = $"Booking Status Update: {booking.BookingStatus}";
            string reasonHtml = !string.IsNullOrEmpty(reason) ? $"<p><strong>Reason:</strong> {reason}</p>" : "";
            
            string bodyContent = $@"
                <h2>Booking Update</h2>
                <p>Hello {customerName},</p>
                <p>Your booking status has changed to: <strong>{booking.BookingStatus}</strong></p>
                <div style='background: #333; padding: 15px; border-radius: 5px; margin: 10px 0;'>
                    <p><strong>Booking Ref:</strong> {booking.BookingId}</p>
                    <p><strong>Car:</strong> {booking.Car?.ModelName} ({booking.Car?.ModelYear})</p>
                    <p><strong>Dates:</strong> {booking.Startdate:dd/MM/yyyy} - {booking.Enddate:dd/MM/yyyy}</p>
                    {reasonHtml}
                </div>
                <p>Contact us if you have questions.</p>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, subject);
            await SendEmailSafeAsync(email, subject, message, "Booking Status Update");
        }

        // 2. PAYMENT & INVOICE EMAILS
        public async Task SendPaymentSuccessEmail(string email, string customerName, Payment payment, Booking booking)
        {
            string subject = "Payment Successful";
            string bodyContent = $@"
                <h2>Payment Confirmed</h2>
                <p>Hello {customerName},</p>
                <p>Thank you! Your payment has been received.</p>
                <div style='background: #333; padding: 15px; border-radius: 5px; margin: 10px 0;'>
                    <p><strong>Booking Ref:</strong> {booking.BookingId}</p>
                    <p><strong>Amount:</strong> {payment.Amount:C}</p>
                    <p><strong>Method:</strong> {payment.PaymentMethod ?? "Unknown"}</p>
                    <p><strong>Date:</strong> {payment.PaymentDate:dd/MM/yyyy HH:mm}</p>
                    <p><strong>Status:</strong> Success</p>
                </div>
                <p>This email serves as your invoice.</p>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, subject);
            await SendEmailSafeAsync(email, subject, message, "Payment Success");
        }

        public async Task SendPaymentFailedEmail(string email, string customerName, decimal amount)
        {
            string subject = "Payment Failed";
            string bodyContent = $@"
                <h2>Payment Failed</h2>
                <p>Hello {customerName},</p>
                <p>We were unable to process your payment of <strong>{amount:C}</strong>.</p>
                <p>Please check your payment details or try a different method.</p>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, subject);
            await SendEmailSafeAsync(email, subject, message, "Payment Failed");
        }

        public async Task SendPaymentCancelledEmail(string email, string customerName, decimal amount)
        {
            string subject = "Payment Cancelled";
            string bodyContent = $@"
                <h2>Payment Cancelled</h2>
                <p>Hello {customerName},</p>
                <p>Your payment of <strong>{amount:C}</strong> has been cancelled.</p>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, subject);
            await SendEmailSafeAsync(email, subject, message, "Payment Cancelled");
        }

        // 3-5. INTERNAL NOTIFICATIONS (Car, Category, Promocode)
        public async Task SendInternalNotification(List<string> recipientEmails, string subject, string title, string detailsHtml, string actorName)
        {
             if (recipientEmails == null || !recipientEmails.Any()) return;

             string bodyContent = $@"
                <h2>{title}</h2>
                <p><strong>Action by:</strong> {actorName}</p>
                <p><strong>Time:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                <div style='background: #333; padding: 15px; border-radius: 5px; margin: 10px 0;'>
                    {detailsHtml}
                </div>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, title);

            foreach (var email in recipientEmails)
            {
                await SendEmailSafeAsync(email, subject, message, "Internal Notification");
            }
        }

        public async Task SendCarUpdateEmail(List<string> emails, Car car, string action, string changedField, string oldValue, string newValue, string actorName)
        {
            string details = $@"
                <p><strong>Car:</strong> {car.ModelName} ({car.ModelYear}) ({car.CarId})</p>
                <p><strong>Action:</strong> {action}</p>
                <p><strong>Field:</strong> {changedField}</p>
                <p><strong>Change:</strong> {oldValue} &rarr; {newValue}</p>";
            
            await SendInternalNotification(emails, $"Car Update: {car.ModelName} ({car.ModelYear})", "Car Update Log", details, actorName);
        }

        public async Task SendCategoryUpdateEmail(List<string> emails, Category category, string action, string oldValue, string newValue, string actorName)
        {
            string details = $@"
                <p><strong>Category:</strong> {category.Name}</p>
                <p><strong>Action:</strong> {action}</p>
                <p><strong>Change:</strong> {oldValue} &rarr; {newValue}</p>";

            await SendInternalNotification(emails, $"Category Update: {category.Name}", "Category Update Log", details, actorName);
        }

        public async Task SendPromocodeUpdateEmail(List<string> emails, Promocode promo, string action, string reason, string actorName)
        {
            string details = $@"
                <p><strong>Promocode:</strong> {promo.Name}</p>
                <p><strong>Action:</strong> {action}</p>
                <p><strong>Reason:</strong> {reason}</p>
                 <p><strong>Discount:</strong> {promo.DiscountPercentage}%</p>";
            await SendInternalNotification(emails, $"Promocode Update: {promo.Name}", "Promocode Update Log", details, actorName);
        }

        // 6. DOCUMENT VERIFICATION
        public async Task SendDocumentVerificationEmail(string email, string customerName, string documentType, string status, string reason, string instructions)
        {
            string subject = $"Document Verification: {status}";
            string bodyContent = $@"
                <h2>Document Status: {status}</h2>
                <p>Hello {customerName},</p>
                <p>Your <strong>{documentType}</strong> has been <strong>{status}</strong>.</p>
                {(string.IsNullOrEmpty(reason) ? "" : $"<p><strong>Reason:</strong> {reason}</p>")}
                {(string.IsNullOrEmpty(instructions) ? "" : $"<p><strong>Next Steps:</strong> {instructions}</p>")}";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, subject);
            await SendEmailSafeAsync(email, subject, message, "Document Verification");
        }

        // 7. BLOCKLIST / ACCOUNT STATUS
        public async Task SendAccountStatusEmail(string email, string customerName, string status, string reason)
        {
            string subject = $"Account Status Update: {status}";
            string bodyContent = $@"
                <h2>Account Status: {status}</h2>
                <p>Hello {customerName},</p>
                <p>Your account status is now: <strong>{status}</strong>.</p>
                <p><strong>Reason:</strong> {reason}</p>
                <p>Please contact support if you believe this is an error.</p>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, subject);
            await SendEmailSafeAsync(email, subject, message, "Account Status");
        }

        public async Task SendAdminAccountStatusNotification(List<string> adminEmails, Customer customer, string action, string reason, string actorName)
        {
             string details = $@"
                <p><strong>Customer:</strong> {customer.Name} ({customer.UserId})</p>
                <p><strong>Action:</strong> {action}</p>
                <p><strong>Reason:</strong> {reason}</p>";
             await SendInternalNotification(adminEmails, $"Customer Status Change: {customer.Name}", "Customer Status Log", details, actorName);
        }

        // 8-9. REMINDERS
        public async Task SendPaymentReminderEmail(string email, string customerName, Booking booking, decimal amountDue)
        {
            string subject = "Payment Reminder: Unpaid Booking";
            string bodyContent = $@"
                <h2>Payment Reminder</h2>
                <p>Hello {customerName},</p>
                <p>You have a pending payment for your booking.</p>
                <div style='background: #333; padding: 15px; border-radius: 5px; margin: 10px 0;'>
                     <p><strong>Booking Ref:</strong> {booking.BookingId}</p>
                     <p><strong>Amount Due:</strong> {amountDue:C}</p>
                     <p><strong>Due Date:</strong> {booking.Startdate:dd/MM/yyyy} (At Pickup)</p>
                </div>
                <p>Please arrange payment to avoid cancellation.</p>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, subject);
            await SendEmailSafeAsync(email, subject, message, "Payment Reminder");
        }

        public async Task SendBookingReminderEmail(string email, string customerName, Booking booking, string type)
        {
            string subject = $"Booking Reminder: {type} Tomorrow";
            string date = type == "Pickup" ? booking.Startdate.ToString("dd/MM/yyyy") : booking.Enddate.ToString("dd/MM/yyyy");
            
            string bodyContent = $@"
                <h2>Booking Reminder: {type}</h2>
                <p>Hello {customerName},</p>
                <p>This is a reminder for your {type} tomorrow.</p>
                <div style='background: #333; padding: 15px; border-radius: 5px; margin: 10px 0;'>
                     <p><strong>Booking Ref:</strong> {booking.BookingId}</p>
                     <p><strong>Car:</strong> {booking.Car?.ModelName} ({booking.Car?.ModelYear})</p>
                     <p><strong>Date:</strong> {date}</p>
                </div>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, subject);
            await SendEmailSafeAsync(email, subject, message, $"Booking Reminder ({type})");
        }
        
        public async Task SendPromocodeExpiredEmail(string email, string customerName, Promocode promo)
        {
             // Usually triggers are internal? User asked for "Promo expired email" under "Internal Employee Notifications".
             // "Send INTERNAL notification emails to ALL EMPLOYEES"
             // Wait, I should double check. Requirement 5: "Internal Employee Notifications - Promocode Updates".
             // Trigger: "Promo code expired".
             // Recipient: "All employees".
             // So this is for employees.
        }

        // Legacy / Existing Methods (Updated to use Safe Send)
        public async Task<int> SendReminderToAllUnverifiedAsync()
        {
            var unverified = await _dbContext.Customers
                .Where(c => !c.IsVerified)
                .ToListAsync();

            int count = 0;
            foreach (var customer in unverified)
            {
                var sent = await SendReminderToCustomerAsync(customer.UserId);
                if (sent) count++;
            }
            return count;
        }

        public async Task<bool> SendReminderToCustomerAsync(int customerId)
        {
            var customer = await _dbContext.Customers.FindAsync(customerId);
            if (customer == null || customer.IsVerified) return false;

            var user = await _userManager.FindByIdAsync(customer.aspNetUserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                 var subject = "Action Required: Verify Your RentACar Account";
                 var bodyContent = $@"
                    <h2>Verify Account</h2>
                    <p>Hello {customer.Name},</p>
                    <p>We noticed you haven't verified your account properly.</p>
                    <p>Please log in to your dashboard and complete your profile.</p>
                    <a href='http://rentacarmohammadmahmoud.shop/Dashboard/Customer' class='btn'>Go to Dashboard</a>";
                 
                 var message = EmailTemplates.GetStandardTemplate(bodyContent, "Action Required");
                 return await SendEmailSafeAsync(user.Email, subject, message, "Unverified Reminder");
            }
            return false;
        }
        
         public async Task<bool> SendOtpEmailAsync(string email, string otp, string name)
        {
            var bodyContent = $@"
                <h2>Security Verification</h2>
                <p>Hello {name},</p>
                <p>Your OTP Code:</p>
                <div style='text-align:center; padding: 20px;'>
                    <span style='font-size: 32px; font-weight: bold; padding: 10px; background: #333; color: #d4af37;'>{otp}</span>
                </div>
                <p>Valid for 5 minutes.</p>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, "Verification Code");
            return await SendEmailSafeAsync(email, "Your Verification Code", message, "OTP");
        }

        public async Task<bool> SendForgotPasswordEmailAsync(string email, string callbackUrl, string name = "User")
        {
            var bodyContent = $@"
                <h2>Reset Your Password</h2>
                <p>Hello {name},</p>
                <p>Click below to reset your password:</p>
                <a href='{callbackUrl}' class='btn'>Reset Password</a>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, "Reset Password");
            return await SendEmailSafeAsync(email, "Reset Password", message, "Password Reset");
        }

        public async Task<bool> SendConfirmationEmailAsync(string email, string callbackUrl, string name = "User")
        {
            var bodyContent = $@"
                <h2>Confirm Your Email</h2>
                <p>Hello {name},</p>
                <p>Click below to confirm your account:</p>
                <a href='{callbackUrl}' class='btn'>Confirm Account</a>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, "Confirm Your Email");
            return await SendEmailSafeAsync(email, "Confirm Your Email", message, "Confirmation Email");
        }

        public async Task SendRecoveryCodesEmailAsync(string email, IEnumerable<string> codes, string name = "User")
        {
            var codesList = string.Join(" ", codes.Select(c => $"<span style='padding:5px; margin:2px; background:#333; color:#d4af37;'>{c}</span>"));
            var bodyContent = $@"
                <h2>New Recovery Codes</h2>
                <p>Hello {name},</p>
                <p>Keep these recovery codes safe:</p>
                <div style='text-align:center; padding: 10px;'>{codesList}</div>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, "Recovery Codes");
            await SendEmailSafeAsync(email, "New Recovery Codes", message, "Recovery Codes");
        }

        // Generic Send for "Send Email" Feature
        public async Task<int> SendAdHocEmailBatchAsync(IEnumerable<string> recipients, string subject, string bodyHtml, Dictionary<string, byte[]> attachments = null)
        {
            var message = EmailTemplates.GetStandardTemplate(bodyHtml, subject);
            int successCount = 0;
            
            // In a real campaign we might want parallel or queue, but for now linear safe send
            foreach (var email in recipients)
            {
                if(await SendEmailSafeAsync(email, subject, message, "AdHoc Campaign", attachments))
                {
                    successCount++;
                }
            }
            return successCount;
        }
        public async Task<int> SendRawEmailBatchAsync(IEnumerable<string> recipients, string subject, string bodyHtml, Dictionary<string, byte[]> attachments = null, string userId = null)
        {
            // Direct send without wrapping in GetStandardTemplate
            int successCount = 0;
            foreach (var email in recipients)
            {
                if(await SendEmailSafeAsync(email, subject, bodyHtml, "Raw Email", attachments, userId))
                {
                    successCount++;
                }
            }
            return successCount;
        }
        public async Task<List<EmailLog>> GetRecentEmailLogsAsync(string userId = null, int count = 50)
        {
            var query = _dbContext.EmailLogs.AsQueryable();
            
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(l => l.CreatedByUserId == userId);
            }

            return await query
                .OrderByDescending(l => l.SentAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<EmailLog> GetEmailLogAsync(int id)
        {
            return await _dbContext.EmailLogs.FindAsync(id);
        }
    }
}
