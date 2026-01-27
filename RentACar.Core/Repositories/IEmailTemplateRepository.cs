using System.Threading.Tasks;
using RentACar.Core.Entities;
using RentACar.Core.Repositories.Base;

namespace RentACar.Core.Repositories
{
    public interface IEmailTemplateRepository : IRepository<EmailTemplate>
    {
        Task<EmailTemplate> GetByKeyAsync(string key);
    }
}
