using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;
using RentACar.Infrastructure.Data;

namespace RentACar.Infrastructure.Data.Repository
{
    public class EmailTemplateRepository : Repository<EmailTemplate>, IEmailTemplateRepository
    {
        public EmailTemplateRepository(RentACarDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<EmailTemplate> GetByKeyAsync(string key)
        {
            return await _dbContext.EmailTemplates.FirstOrDefaultAsync(x => x.TemplateKey == key);
        }
    }
}
