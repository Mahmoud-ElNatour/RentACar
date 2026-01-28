using System.Threading.Tasks;

namespace RentACar.Core.Repositories
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}
