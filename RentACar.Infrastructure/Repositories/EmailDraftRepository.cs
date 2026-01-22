using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Repositories
{
    public class EmailDraftRepository : Repository<EmailDraft>, IEmailDraftRepository
    {
        public EmailDraftRepository(RentACarDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<EmailDraft>> GetDraftsByUserIdAsync(string userId)
        {
            return await _dbContext.EmailDrafts
                .Where(d => d.CreatedByUserId == userId)
                .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
                .ToListAsync();
        }

        public async Task<EmailDraft> GetDraftByIdAndUserIdAsync(int id, string userId)
        {
            return await _dbContext.EmailDrafts
                .FirstOrDefaultAsync(d => d.Id == id && d.CreatedByUserId == userId);
        }
    }
}
