using System.Threading.Tasks;

namespace RentACar.Core.Repositories
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message, Dictionary<string, byte[]> attachments = null, string? fromEmail = null, string? fromName = null);
    }
}
