using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repository;

public class DriverLocationRepository : Repository<DriverLocation>, IDriverLocationRepository
{
    private readonly RentACarDbContext _dbContext;

    public DriverLocationRepository(RentACarDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<DriverLocation?> GetLatestByBookingIdAsync(int bookingId)
    {
        return _dbContext.DriverLocations
            .Where(l => l.BookingId == bookingId)
            .OrderByDescending(l => l.LastUpdatedUtc)
            .FirstOrDefaultAsync();
    }

    public Task<List<DriverLocation>> GetByDriverIdAsync(int driverId)
    {
        return _dbContext.DriverLocations
            .Where(l => l.DriverId == driverId)
            .OrderByDescending(l => l.LastUpdatedUtc)
            .ToListAsync();
    }
}
