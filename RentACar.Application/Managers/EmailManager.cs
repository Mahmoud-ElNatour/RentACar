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
                <a href='http://rentacarmohammadmahmoud.shop/Dashboard/Customer' class='btn'>Go to Dashboard</a>
                <br><br>
                <p>Best Regards,<br>RentACar Team</p>";

            var message = EmailTemplates.GetStandardTemplate(bodyContent, "Action Required");

            await _emailService.SendEmailAsync(email, subject, message);
        }
    }
}
