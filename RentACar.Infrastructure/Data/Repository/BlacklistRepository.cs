using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repository
{
    public class BlacklistRepository : Repository<BlackList>, IBlacklistRepository
    {
        private RentACarDbContext _dbContext;
        public BlacklistRepository(RentACarDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;

        }

        public async Task<BlackList?> GetByUserIdAsync(string userId)
        {
            return await _dbContext.BlackLists.FirstOrDefaultAsync(bl => bl.UserId == userId);
        }

    }
}
