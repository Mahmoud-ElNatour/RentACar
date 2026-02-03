using System.Linq;
using System.Threading.Tasks;
using RentACar.Core.Entities;

namespace RentACar.Core.Repositories
{
    public interface ISupportConversationRepository
    {
        IQueryable<SupportConversation> Query();
        Task<SupportConversation?> GetByIdAsync(int id);
        Task<SupportConversation> AddAsync(SupportConversation entity);
        Task UpdateAsync(SupportConversation entity);
        Task SaveChangesAsync();
    }
}
