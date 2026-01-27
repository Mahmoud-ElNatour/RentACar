using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;
using RentACar.Core.Repositories.Base;

namespace RentACar.Core.Repositories
{
    public interface IEmailDraftRepository : IRepository<EmailDraft>
    {
        Task<IEnumerable<EmailDraft>> GetDraftsByUserIdAsync(string userId);
        Task<EmailDraft> GetDraftByIdAndUserIdAsync(int id, string userId);
        Task DeleteDraftsByUserIdAsync(string userId);
    }
}
