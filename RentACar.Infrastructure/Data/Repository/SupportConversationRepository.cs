using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repository
{
    public class SupportConversationRepository : Repository<SupportConversation>, ISupportConversationRepository
    {
        public SupportConversationRepository(RentACarDbContext dbContext) : base(dbContext)
        {
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
