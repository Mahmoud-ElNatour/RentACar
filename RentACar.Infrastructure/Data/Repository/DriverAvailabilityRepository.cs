using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repository;

public class DriverAvailabilityRepository : Repository<DriverAvailability>, IDriverAvailabilityRepository
{
    private readonly RentACarDbContext _dbContext;

    public DriverAvailabilityRepository(RentACarDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<DriverAvailability>> GetByDriverIdAsync(int driverId)
    {
        return _dbContext.DriverAvailabilities
            .Where(a => a.DriverId == driverId)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }
}
