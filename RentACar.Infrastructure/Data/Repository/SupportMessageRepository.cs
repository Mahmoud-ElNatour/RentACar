using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repository
{
    public class SupportMessageRepository : Repository<SupportMessage>, ISupportMessageRepository
    {
        public SupportMessageRepository(RentACarDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<SupportMessage>> GetByConversationIdAsync(int conversationId)
        {
            return await _dbContext.SupportMessages
                .Where(m => m.SupportConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
