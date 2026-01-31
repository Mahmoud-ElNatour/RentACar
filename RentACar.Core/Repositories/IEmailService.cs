using System.Threading.Tasks;

namespace RentACar.Core.Repositories
{
    public interface IEmailService
    {
<<<<<<< HEAD
        Task SendEmailAsync(string toEmail, string subject, string message, Dictionary<string, byte[]> attachments = null, string? fromEmail = null, string? fromName = null);
=======
        Task SendEmailAsync(string toEmail, string subject, string message);
>>>>>>> Mahmoud-V3
    }
}
