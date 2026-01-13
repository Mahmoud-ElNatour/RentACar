using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data;

namespace RentACar.Application.Managers
{
    public class EmailManager
    {
        private readonly IEmailService _emailService;
        private readonly RentACarDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;

        public EmailManager(
            IEmailService emailService,
            RentACarDbContext dbContext,
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
            }
            return false;
        }

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
    }
}
