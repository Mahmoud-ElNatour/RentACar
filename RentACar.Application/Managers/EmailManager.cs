using System.Threading.Tasks;
using RentACar.Core.Repositories;

namespace RentACar.Application.Managers
{
    public class EmailManager
    {
        private readonly IEmailService _emailService;

        public EmailManager(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task SendReminderToUnverifiedAsync(string email, string name)
        {
            var subject = "Action Required: Verify Your RentACar Account";
            var message = $@"
                <h3>Hello {name},</h3>
                <p>We noticed you haven't verified your account properly (e.g., missing ID or documentation).</p>
                <p>Please log in to your dashboard and complete your profile to start booking cars.</p>
                <br>
                <p>Best Regards,<br>RentACar Team</p>";

            await _emailService.SendEmailAsync(email, subject, message);
        }
    }
}
