using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RentACar.Core.Entities;

namespace RentACar.Core.Repositories
{
    public interface ISupportMessageRepository
    {
        IQueryable<SupportMessage> Query();
        Task<SupportMessage> AddAsync(SupportMessage entity);
        Task<List<SupportMessage>> GetByConversationIdAsync(int conversationId);
        Task SaveChangesAsync();
    }
}
