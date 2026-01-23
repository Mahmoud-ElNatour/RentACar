using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Infrastructure.Data.Repository;

public class DriverRepository : IDriverRepository
{
    private readonly RentACarDbContext _dbContext;

    public DriverRepository(RentACarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Driver?> GetByIdAsync(int id)
    {
        return await _dbContext.Drivers
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.DriverId == id);
    }

    public async Task<Driver?> GetByAspNetUserIdAsync(string aspNetUserId)
    {
        return await _dbContext.Drivers
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.AspNetUserId == aspNetUserId);
    }

    public async Task<List<Driver>> GetAllAsync()
    {
        return await _dbContext.Drivers
            .Include(d => d.User)
            .ToListAsync();
    }

    public async Task<List<Driver>> GetActiveAsync()
    {
        return await _dbContext.Drivers
            .Include(d => d.User)
            .Where(d => d.IsActive)
            .ToListAsync();
    }

    public async Task AddAsync(Driver driver)
    {
        await _dbContext.Drivers.AddAsync(driver);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Driver driver)
    {
        _dbContext.Entry(driver).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var driver = await _dbContext.Drivers.FindAsync(id);
        if (driver != null)
        {
            _dbContext.Drivers.Remove(driver);
            await _dbContext.SaveChangesAsync();
        }
    }
}
