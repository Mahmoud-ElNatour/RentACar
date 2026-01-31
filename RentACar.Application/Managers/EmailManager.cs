using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data;
<<<<<<< HEAD
using RentACar.Core.Entities;
=======
>>>>>>> Mahmoud-V3

namespace RentACar.Application.Managers
{
    public class EmailManager
    {
        private readonly IEmailService _emailService;
        private readonly RentACarDbContext _dbContext;
<<<<<<< HEAD
        private readonly ApplicationDbContext _appDbContext; // Added for FeatureConfig
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AuditLogManager _auditLogManager;
        private readonly EmailTemplateManager _templateManager;
=======
        private readonly UserManager<IdentityUser> _userManager;
>>>>>>> Mahmoud-V3

        public EmailManager(
            IEmailService emailService,
            RentACarDbContext dbContext,
<<<<<<< HEAD
            ApplicationDbContext appDbContext,
            UserManager<IdentityUser> userManager,
            AuditLogManager auditLogManager,
            EmailTemplateManager templateManager)
        {
            _emailService = emailService;
            _dbContext = dbContext;
            _appDbContext = appDbContext;
            _userManager = userManager;
            _auditLogManager = auditLogManager;
            _templateManager = templateManager;
        }

        private async Task<bool> SendTemplatedEmailAsync(string recipient, string featureKey, string defaultTemplateKey, Dictionary<string, string> placeholders, string fallbackSubject = "Notification", string fallbackBody = "", string emailType = "Notification")
        {
            if (string.IsNullOrEmpty(recipient)) return false;

            string subject = fallbackSubject;
            string body = fallbackBody;
            string templateUsed = defaultTemplateKey;
            string? fromEmail = null;
            string? fromName = null;

            try
            {
                // 1. Resolve effective Template & Sender from Feature Config
                var featureConfig = await _appDbContext.EmailFeatureConfigs
                    .Include(f => f.SenderIdentity)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.FeatureKey == featureKey);

                if (featureConfig != null)
                {
                    if (!featureConfig.Enabled) return false; // Feature disabled

                    // Resolve Template
                    if (!string.IsNullOrEmpty(featureConfig.TemplateKey))
                        templateUsed = featureConfig.TemplateKey;

                    // Resolve Sender
                    if (featureConfig.SenderIdentity != null && featureConfig.SenderIdentity.IsActive)
                    {
                        fromEmail = featureConfig.SenderIdentity.FromEmail;
                        fromName = featureConfig.SenderIdentity.DisplayName;
                    }
                }

                // 2. Fetch Template
                var template = await _templateManager.GetTemplateByKeyAsync(templateUsed);
                if (template != null && template.IsActive)
                {
                    subject = template.Subject;
                    body = template.Body;

                    // 3. Replace Placeholders
                    foreach (var kvp in placeholders)
                    {
                        var key = "{{" + kvp.Key + "}}";
                        if (subject.Contains(key)) subject = subject.Replace(key, kvp.Value);
                        if (body.Contains(key)) body = body.Replace(key, kvp.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback to legacy/args
                // Log warning
            }

            return await SendEmailSafeAsync(recipient, subject, body, emailType, null, null, templateUsed, fromEmail, fromName);
        }

        public async Task<bool> SendEmailSafeAsync(string recipient, string subject, string message, string emailType, Dictionary<string, byte[]> attachments = null, string userId = null, string templateKey = "", string? fromEmail = null, string? fromName = null)
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
                CreatedByUserId = string.IsNullOrEmpty(userId) ? null : userId,
                TemplateKey = templateKey,
                LastError = "",
                Status = "Pending"
            };

            try
            {   
                await _emailService.SendEmailAsync(recipient, subject ?? "", message ?? "", attachments, fromEmail, fromName);
                
                emailLog.Status = "Sent";
                emailLog.SentAt = DateTime.UtcNow;
                
                await _auditLogManager.LogEventAsync("EmailSent", "Notification", recipient, $"Sent {emailType} to {recipient} (From: {fromEmail ?? "Default"})", status: "Success");
            }
            catch (Exception ex)
            {
                emailLog.Status = "Failed";
                emailLog.LastError = ex.Message;
                
                await _auditLogManager.LogEventAsync("EmailFailed", "Notification", recipient, $"Failed to send {emailType}", status: "Failed", failureReason: ex.Message);
            }
            
            try 
            {
                _dbContext.EmailLogs.Add(emailLog);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
               // Silent fail on log save
            }

            return emailLog.Status == "Sent";
        }

        // 1. BOOKING STATUS -> BookingStatusChanged
        public async Task SendBookingStatusEmail(string email, string customerName, Booking booking, string reason = null)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "CustomerName", customerName },
                { "BookingId", booking.BookingId.ToString() },
                { "BookingStatus", booking.BookingStatus },
                { "CarModel", booking.Car != null ? $"{booking.Car.ModelName} ({booking.Car.ModelYear})" : "Vehicle" },
                { "StartDate", booking.Startdate.ToString("dd MMM yyyy") },
                { "EndDate", booking.Enddate.ToString("dd MMM yyyy") },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            string reasonHtml = !string.IsNullOrEmpty(reason) ? $"<p><strong>Reason:</strong> {reason}</p>" : "";
            string fallbackBody = $@"<h2>Booking Update</h2><p>Status: {booking.BookingStatus}</p>{reasonHtml}";
            fallbackBody = EmailTemplates.GetStandardTemplate(fallbackBody, $"Booking: {booking.BookingStatus}");

            await SendTemplatedEmailAsync(email, "BookingStatusChanged", "CUST-BOOK-UPDATE", placeholders, $"Booking Update: {booking.BookingStatus}", fallbackBody, "Booking Update");
        }

        // 2. PAYMENT SUCCESS -> PaymentInvoice
        public async Task SendPaymentSuccessEmail(string email, string customerName, Payment payment, Booking booking)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "CustomerName", customerName },
                { "BookingId", booking?.BookingId.ToString() ?? "N/A" },
                { "Amount", $"{payment.Amount:C}" },
                { "PaymentDate", payment.PaymentDate.ToString("dd MMM yyyy") },
                { "PaymentMethod", payment.PaymentMethod ?? "Unknown" },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            string fallbackBody = $@"<h2>Payment Confirmed</h2><p>Recieved: {payment.Amount:C}</p>";
            fallbackBody = EmailTemplates.GetStandardTemplate(fallbackBody, "Payment Success");

            // Assuming default key "CUST-PAY-SUCCESS" (placeholder) or generic.
            // Using "CUST-BOOK-UPDATE" as placeholder isn't ideal but no "Pay Success" template exists in user set.
            // Keeping it empty string as default implies "Dynamic Feature Config MUST be set" or "Use Fallback".
            // Since we don't have a template in DB for this, we rely on Fallback OR user mapping it to a custom one.
            await SendTemplatedEmailAsync(email, "PaymentInvoice", "", placeholders, "Payment Successful", fallbackBody, "Payment Invoice");
        }

        // 3. PAYMENT FAILED -> PaymentFailed
        public async Task SendPaymentFailedEmail(string email, string customerName, decimal amount, int? bookingId = null, string paymentUrl = "#")
        {
            var placeholders = new Dictionary<string, string>
            {
                { "CustomerName", customerName },
                { "Amount", $"{amount:C}" },
                { "BookingId", bookingId.HasValue ? bookingId.ToString() : "?" },
                { "PaymentUrl", paymentUrl }, // Caller needs to provide this
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            string fallbackBody = $@"<h2>Payment Failed</h2><p>Amount: {amount:C}</p>";
            fallbackBody = EmailTemplates.GetStandardTemplate(fallbackBody, "Payment Failed");

            await SendTemplatedEmailAsync(email, "PaymentFailed", "CUST-PAY-FAILED", placeholders, "Action Required: Payment Failed", fallbackBody, "Payment Failed Alert");
        }

        public async Task SendPaymentCancelledEmail(string email, string customerName, decimal amount)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "CustomerName", customerName },
                { "Amount", $"{amount:C}" },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };
            
            string fallbackBody = $@"<h2>Payment Cancelled</h2><p>Amount: {amount:C}</p>";
            fallbackBody = EmailTemplates.GetStandardTemplate(fallbackBody, "Payment Cancelled");

            // No Feature for Payment Cancelled explicitly in seed, maybe "PaymentFailed" or new one?
            // Using "PaymentFailed" as proxy? No. 
            // Just use ad-hoc key "PaymentCancelled" (it won't match seed, so falls back to default empty, usage of fallback body).
            await SendTemplatedEmailAsync(email, "PaymentCancelled", "", placeholders, "Payment Cancelled", fallbackBody, "Payment Cancelled");
        }

        // 4. INTERNAL
        public async Task SendInternalNotification(List<string> recipientEmails, string subject, string title, string detailsHtml, string actorName)
        {
             if (recipientEmails == null || !recipientEmails.Any()) return;
             var message = EmailTemplates.GetStandardTemplate(detailsHtml, title);
             foreach (var email in recipientEmails)
             {
                 await SendEmailSafeAsync(email, subject, message, "Internal Notification");
             }
        }

        // INT-CAR-UPD -> CarUpdatedInternal
        public async Task SendCarUpdateEmail(List<string> emails, Car car, string action, string changedField, string oldValue, string newValue, string actorName)
        {
            if (emails == null || !emails.Any()) return;

            var placeholders = new Dictionary<string, string>
            {
                { "UpdatedBy", actorName },
                { "CarModel", car != null ? $"{car.ModelName} ({car.ModelYear})" : "Unknown Car" },
                { "PlateNumber", car?.PlateNumber ?? "N/A" },
                { "ChangeSummary", $"{action}: {changedField} changed from '{oldValue}' to '{newValue}'" },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            foreach (var email in emails)
            {
                await SendTemplatedEmailAsync(email, "CarUpdatedInternal", "INT-CAR-UPD", placeholders, "Fleet Update", "", "Internal Car Update");
            }
        }

        // INT-CAT-UPD -> CategoryUpdatedInternal
        public async Task SendCategoryUpdateEmail(List<string> emails, Category category, string action, string oldValue, string newValue, string actorName)
        {
            if (emails == null || !emails.Any()) return;

            var placeholders = new Dictionary<string, string>
            {
                { "CategoryName", category?.Name ?? "Unknown" },
                { "NewPrice", newValue },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            foreach (var email in emails)
            {
                await SendTemplatedEmailAsync(email, "CategoryUpdatedInternal", "INT-CAT-UPD", placeholders, "Category Pricing Update", "", "Internal Category Update");
            }
        }

        // INT-PROMOC-UPD -> PromocodeUpdatedInternal
        public async Task SendPromocodeUpdateEmail(List<string> emails, Promocode promo, string action, string reason, string actorName)
        {
            if (emails == null || !emails.Any()) return;

            var placeholders = new Dictionary<string, string>
            {
                { "PromoCode", promo?.Name ?? "N/A" },
                { "ChangeDetail", $"{action}: {reason}. Discount: {promo?.DiscountPercentage}%" },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            foreach (var email in emails)
            {
                await SendTemplatedEmailAsync(email, "PromocodeUpdatedInternal", "INT-PROMOC-UPD", placeholders, "Promo Modified", "", "Internal Promo Update");
            }
        }

        // 5. DOC VERIFY -> DocumentStatusUpdate
        public async Task SendDocumentVerificationEmail(string email, string customerName, string documentType, string status, string reason, string instructions)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "CustomerName", customerName },
                { "DocumentType", documentType },
                { "DocumentStatus", status },
                { "RejectionReason", reason ?? instructions ?? "" },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            string fallback = $"<h2>Document {status}</h2><p>{reason}</p>";
            fallback = EmailTemplates.GetStandardTemplate(fallback, "Document Update");

            await SendTemplatedEmailAsync(email, "DocumentStatusUpdate", "CUST-DOC-VERIFY", placeholders, $"Document Status: {status}", fallback, "Document Verification");
        }

        // 6. ACCOUNT STATUS -> AccountStatusChanged
        public async Task SendAccountStatusEmail(string email, string customerName, string status, string reason)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "CustomerName", customerName },
                { "AccountStatus", status },
                { "StatusReason", reason },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            string fallback = $"<h2>Account {status}</h2><p>{reason}</p>";
            fallback = EmailTemplates.GetStandardTemplate(fallback, "Account Status");

            await SendTemplatedEmailAsync(email, "AccountStatusChanged", "AUTH-ACC-STATUS", placeholders, $"Account Status: {status}", fallback, "Account Status");
        }

        public async Task SendAdminAccountStatusNotification(List<string> adminEmails, Customer customer, string action, string reason, string actorName)
        {
             // Generic fallback
             string details = $@"<p>Customer: {customer.Name}</p><p>{action}</p><p>{reason}</p>";
             await SendInternalNotification(adminEmails, $"Customer Status: {customer.Name}", "Status Change", details, actorName);
        }

        // 7. PAYMENT REMINDER -> PaymentReminder
        public async Task SendPaymentReminderEmail(string email, string customerName, Booking booking, decimal amountDue)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "CustomerName", customerName },
                { "BookingId", booking.BookingId.ToString() },
                { "Amount", $"{amountDue:C}" },
                { "PaymentUrl", $"/Payment/Pay?bookingId={booking.BookingId}" },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            string fallback = $"<h2>Payment Reminder</h2><p>Due: {amountDue:C}</p>";
            fallback = EmailTemplates.GetStandardTemplate(fallback, "Payment Reminder");

            await SendTemplatedEmailAsync(email, "PaymentReminder", "REM-PAY-GENERIC", placeholders, "Payment Reminder", fallback, "Payment Due Reminder");
        }

        // 8. BOOKING REMINDER -> PickupReminder / ReturnReminder
        public async Task SendBookingReminderEmail(string email, string customerName, Booking booking, string type)
        {
            string featureKey = type == "Pickup" ? "PickupReminder" : "ReturnReminder";
            string defaultKey = type == "Pickup" ? "REM-PICK-INSTR" : "REM-RET-INSTR";
            
            var placeholders = new Dictionary<string, string>
            {
                { "CustomerName", customerName },
                { "BookingId", booking.BookingId.ToString() },
                { "CarModel", booking.Car != null ? $"{booking.Car.ModelName}" : "Vehicle" },
                { "PickupTime", booking.Startdate.ToString("dd MMM yyyy") + " (Check local time)" },
                { "PickupLocation", "Main Office" }, // Ideally fetch from booking
                { "ReturnDate", booking.Enddate.ToString("dd MMM yyyy") },
                { "ReturnLocation", "Main Office" },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            string fallback = $"<h2>Reminder: {type}</h2>";
            fallback = EmailTemplates.GetStandardTemplate(fallback, "Booking Reminder");

            await SendTemplatedEmailAsync(email, featureKey, defaultKey, placeholders, $"Booking Reminder: {type} Tomorrow", fallback, $"Booking Reminder ({type})");
        }
        
        public async Task SendPromocodeExpiredEmail(string email, string customerName, Promocode promo)
        {
             // NO-OP or implement similarly if needed
        }

        // Legacy / Standard
        public async Task<int> SendReminderToAllUnverifiedAsync()
        {
            var unverified = await _dbContext.Customers.Where(c => !c.IsVerified).ToListAsync();
            int count = 0;
            foreach (var customer in unverified)
            {
                if(await SendReminderToCustomerAsync(customer.UserId)) count++;
=======
            UserManager<IdentityUser> userManager)
        {
            _emailService = emailService;
            _dbContext = dbContext;
            _userManager = userManager;
        }

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
>>>>>>> Mahmoud-V3
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
<<<<<<< HEAD
                 var placeholders = new Dictionary<string, string>
                 {
                     { "CustomerName", customer.Name },
                     { "Year", DateTime.UtcNow.Year.ToString() }
                 };

                 string fallback = $"<h2>Verify Account</h2><p>Hello {customer.Name}, please upload your documents to verify your account.</p>";
                 fallback = EmailTemplates.GetStandardTemplate(fallback, "Action Required");

                 // Feature Key: UnverifiedDocsReminder
                 return await SendTemplatedEmailAsync(user.Email, "UnverifiedDocsReminder", "REM-UNVERIFIED-DOCS", placeholders, "Action Required: Verify Account", fallback, "Unverified Reminder");
=======
                try
                {
                    await SendEmailInternalAsync(user.Email, customer.Name);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email to {user.Email}: {ex.Message}");
                    return false;
                }
>>>>>>> Mahmoud-V3
            }
            return false;
        }

<<<<<<< HEAD
        public async Task SendPaymentFailedEmail(string email, string customerName, int bookingId, decimal amount)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "CustomerName", customerName },
                { "BookingId", bookingId.ToString() },
                { "Amount", $"{amount:C}" },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            string fallback = $"<h2>Payment Failed</h2><p>Transaction for {amount:C} failed.</p>";
            fallback = EmailTemplates.GetStandardTemplate(fallback, "Payment Failed");

            await SendTemplatedEmailAsync(email, "PaymentFailed", "PAY-FAILED", placeholders, "Payment Failed", fallback, "Payment Failed");
        }
        
        // 9. OTP -> Otp2FA
         public async Task<bool> SendOtpEmailAsync(string email, string otp, string name)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "CustomerName", name },
                { "OtpCode", otp },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            string fallback = $"<h2>OTP: {otp}</h2>";
            fallback = EmailTemplates.GetStandardTemplate(fallback, "Security Code");

            // Key: AUTH-OTP-SECURE
            return await SendTemplatedEmailAsync(email, "Otp2FA", "AUTH-OTP-SECURE", placeholders, "Your Secure Login Code", fallback, "One-Time Password");
        }

        // 10. FORGOT PASSWORD -> ForgotPassword
        public async Task<bool> SendForgotPasswordEmailAsync(string email, string callbackUrl, string name = "User")
        {
            var placeholders = new Dictionary<string, string>
            {
                { "CustomerName", name },
                { "ActionUrl", callbackUrl },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            string fallback = $"<h2>Reset Password</h2><a href='{callbackUrl}'>Click here</a>";
            fallback = EmailTemplates.GetStandardTemplate(fallback, "Reset Password");

            return await SendTemplatedEmailAsync(email, "ForgotPassword", "AUTH-RESET-V1", placeholders, "Reset your password", fallback, "Forgot Password");
        }

        // 11. CONFIRM EMAIL -> VerifyEmail
        public async Task<bool> SendConfirmationEmailAsync(string email, string callbackUrl, string name = "User")
        {
            var placeholders = new Dictionary<string, string>
            {
                { "CustomerName", name },
                { "VerifyUrl", callbackUrl },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            string fallback = $"<h2>Confirm Email</h2><a href='{callbackUrl}'>Click here</a>";
            fallback = EmailTemplates.GetStandardTemplate(fallback, "Confirm Email");

            return await SendTemplatedEmailAsync(email, "VerifyEmail", "AUTH-VERIFY-LINK", placeholders, "Confirm Your Email", fallback, "Verify Email Address");
        }

        // 12. ADMIN RESET PASSWORD -> AdminResetPassword
        public async Task SendAdminResetPasswordEmail(string email, string newPassword, string name = "User")
        {
            var placeholders = new Dictionary<string, string>
            {
                { "CustomerName", name },
                { "NewPassword", newPassword },
                { "LoginUrl", "/Identity/Account/Login" },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            string fallback = $@"<h2>Password Reset</h2><p>Your password has been reset by an administrator.</p><p><strong>New Password:</strong> {newPassword}</p><p>Please change this password after logging in.</p>";
            fallback = EmailTemplates.GetStandardTemplate(fallback, "Security Update");

            await SendTemplatedEmailAsync(email, "AdminResetPassword", "ADMIN-RESET-PWD", placeholders, "Your Password Has Been Reset", fallback, "Admin Reset Password");
        }

        // 13. PASSWORD CHANGED -> PasswordChanged
        public async Task SendPasswordChangedNotification(string email, string name = "User")
        {
            var placeholders = new Dictionary<string, string>
            {
                { "CustomerName", name },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            string fallback = $@"<h2>Security Alert</h2><p>Your password was recently changed.</p><p>If this wasn't you, please contact support immediately.</p>";
            fallback = EmailTemplates.GetStandardTemplate(fallback, "Security Alert");

            await SendTemplatedEmailAsync(email, "PasswordChanged", "AUTH-PWD-CHANGE", placeholders, "Your Password Was Changed", fallback, "Password Changed Notification");
        }

        public async Task SendRecoveryCodesEmailAsync(string email, IEnumerable<string> codes, string name = "User")
        {
            var codesList = string.Join(" ", codes.Select(c => $"<span style='padding:5px; margin:2px; background:#333; color:#d4af37;'>{c}</span>"));
            var bodyContent = $@"<h2>Recovery Codes</h2><p>{codesList}</p>";
            var message = EmailTemplates.GetStandardTemplate(bodyContent, "Recovery Codes");
            await SendEmailSafeAsync(email, "New Recovery Codes", message, "Recovery Codes");
        }

        // Generic Send for "Send Email" Feature
        public async Task<int> SendAdHocEmailBatchAsync(IEnumerable<string> recipients, string subject, string bodyHtml, Dictionary<string, byte[]> attachments = null)
        {
            var message = EmailTemplates.GetStandardTemplate(bodyHtml, subject);
            int successCount = 0;
            foreach (var email in recipients)
            {
                if(await SendEmailSafeAsync(email, subject, message, "AdHoc Campaign", attachments)) successCount++;
            }
            return successCount;
        }
        
        public async Task<int> SendRawEmailBatchAsync(IEnumerable<string> recipients, string subject, string bodyHtml, Dictionary<string, byte[]> attachments = null, string userId = null)
        {
            int successCount = 0;
            foreach (var email in recipients)
            {
                if(await SendEmailSafeAsync(email, subject, bodyHtml, "Raw Email", attachments, userId)) successCount++;
            }
            return successCount;
        }

        public async Task<bool> SendTestEmailAsync(string recipient, string subject, string body)
        {
            return await SendEmailSafeAsync(recipient, subject, body, "Test Provider Email");
        }
        
        public async Task<List<EmailLog>> GetRecentEmailLogsAsync(string userId = null, int count = 50)
        {
            var query = _dbContext.EmailLogs.AsQueryable();
            if (!string.IsNullOrEmpty(userId)) query = query.Where(l => l.CreatedByUserId == userId);
            return await query.OrderByDescending(l => l.SentAt).Take(count).ToListAsync();
        }

        public async Task<EmailLog> GetEmailLogAsync(int id)
        {
            return await _dbContext.EmailLogs.FindAsync(id);
=======
        private async Task SendEmailInternalAsync(string email, string name)
        {
            var subject = "Action Required: Verify Your RentACar Account";
            var bodyContent = $@"
                <h2>Verify Account</h2>
                <p>Hello {name},</p>
                <p>We noticed you haven't verified your account properly (e.g., missing ID or documentation).</p>
                <p>Please log in to your dashboard and complete your profile to start booking cars.</p>
                <p>Please log in to your dashboard and complete your profile to start booking cars.</p>
                <a href='http://rentacarmohammadmahmoud.shop/Dashboard/Customer' class='btn' style='display: inline-block; padding: 12px 24px; background-color: #d4af37; color: #000000; text-decoration: none; border-radius: 4px; font-weight: bold; margin-top: 20px;'>Go to Dashboard</a>
                <br><br>
                <p>Best Regards,<br>RentACar Team</p>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, "Action Required");

            await _emailService.SendEmailAsync(email, subject, message);
        }

    public async Task<bool> SendOtpEmailAsync(string email, string otp, string name)
        {
            var bodyContent = $@"
                <h2>Security Verification</h2>
                <p>Hello {name},</p>
                <p>You requested to change your password or security settings.</p>
                <p>Please use the following One-Time Password (OTP) to complete the verification:</p>
                <div style='text-align:center; padding: 20px;'>
                    <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #d4af37;'>{otp}</span>
                </div>
                <p>This code is valid for 5 minutes. Do not share this code with anyone.</p>
                <br>
                <p>If you did not request this code, please ignore this email.</p>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, "Verification Code");
            
            try
            {
                await _emailService.SendEmailAsync(email, "Your Verification Code", message);
                return true;
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error sending OTP: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendForgotPasswordEmailAsync(string email, string callbackUrl, string name = "User")
        {
            var bodyContent = $@"
                <h2>Reset Your Password</h2>
                <p>Hello {name},</p>
                <p>We received a request to reset your password.</p>
                <p>Please click the button below to reset your password:</p>
                <a href='{callbackUrl}' class='btn' style='display: inline-block; padding: 12px 24px; background-color: #d4af37; color: #000000; text-decoration: none; border-radius: 4px; font-weight: bold; margin-top: 20px;'>Reset Password</a>
                <br><br>
                <p>If you did not request a password reset, you can safely ignore this email.</p>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, "Reset Password");

            try
            {
                await _emailService.SendEmailAsync(email, "Reset Password", message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending Forgot Password email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendConfirmationEmailAsync(string email, string callbackUrl, string name = "User")
        {
            var bodyContent = $@"
                <h2>Confirm Your Email</h2>
                <p>Hello {name},</p>
                <p>Thank you for registering with RentACar.</p>
                <p>Please confirm your account by clicking the button below:</p>
                <a href='{callbackUrl}' class='btn' style='display: inline-block; padding: 12px 24px; background-color: #d4af37; color: #000000; text-decoration: none; border-radius: 4px; font-weight: bold; margin-top: 20px;'>Confirm Account</a>
                <br><br>
                <p>If you did not create an account, no further action is required.</p>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, "Confirm Your Email");

            try
            {
                await _emailService.SendEmailAsync(email, "Confirm Your Email", message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending Confirmation email: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> SendRecoveryCodesEmailAsync(string email, IEnumerable<string> codes, string name = "User")
        {
            var codesList = string.Join("</div><div style='padding:10px; border:1px solid #444; margin:5px; display:inline-block; font-family:monospace; color:#d4af37; background:#222; border-radius:4px;'>", codes);
            codesList = $"<div style='text-align:center; padding: 20px;'><div style='padding:10px; border:1px solid #444; margin:5px; display:inline-block; font-family:monospace; color:#d4af37; background:#222; border-radius:4px;'>{codesList}</div></div>";

            var bodyContent = $@"
                <h2>New Recovery Codes</h2>
                <p>Hello {name},</p>
                <p>You have generated a new set of recovery codes for your RentACar account.</p>
                <p><strong>These codes are the only way to access your account if you lose your 2FA device.</strong></p>
                <p>Keep them safe and secure.</p>
                {codesList}
                <p><strong>Note:</strong> Generating these codes has invalidated any previous codes you may have saved.</p>
                <br>
                <p>If you did not perform this action, please secure your account immediately.</p>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, "Recovery Codes");

            try
            {
                await _emailService.SendEmailAsync(email, "New Recovery Codes", message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending Recovery Codes email: {ex.Message}");
                return false;
            }
>>>>>>> Mahmoud-V3
        }
    }
}
