using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Infrastructure.Data.Repository;

public class DriverAvailabilityRepository : IDriverAvailabilityRepository
{
    private readonly RentACarDbContext _dbContext;

    public DriverAvailabilityRepository(RentACarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<DriverAvailability>> GetByDriverIdAsync(int driverId)
    {
        return await _dbContext.DriverAvailabilities
            .Where(a => a.DriverId == driverId)
            .OrderByDescending(a => a.StartDateTime)
            .ToListAsync();
    }

    public async Task AddAsync(DriverAvailability availability)
    {
        await _dbContext.DriverAvailabilities.AddAsync(availability);
        await _dbContext.SaveChangesAsync();
    }
}
