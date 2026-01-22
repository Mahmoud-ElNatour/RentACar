using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repository;

public class DriverRepository : Repository<Driver>, IDriverRepository
{
    private readonly RentACarDbContext _dbContext;

    public DriverRepository(RentACarDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Driver?> GetByAspNetUserIdAsync(string aspNetUserId)
    {
        return _dbContext.Drivers
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.AspNetUserId == aspNetUserId);
    }

    public Task<List<Driver>> GetActiveDriversAsync()
    {
        return _dbContext.Drivers
            .Include(d => d.User)
            .Where(d => d.IsActive)
            .ToListAsync();
    }
}
