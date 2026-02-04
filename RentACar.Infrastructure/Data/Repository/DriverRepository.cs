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
            .Include(d => d.Employee)
            .FirstOrDefaultAsync(d => d.DriverId == id);
    }

    public async Task<Driver?> GetByEmployeeIdAsync(int employeeId)
    {
        return await _dbContext.Drivers
            .Include(d => d.User)
            .Include(d => d.Employee)
            .FirstOrDefaultAsync(d => d.EmployeeId == employeeId);
    }

    public async Task<Driver?> GetByAspNetUserIdAsync(string aspNetUserId)
    {
        return await _dbContext.Drivers
            .Include(d => d.User)
            .Include(d => d.Employee)
            .FirstOrDefaultAsync(d => d.AspNetUserId == aspNetUserId);
    }

    public async Task<List<Driver>> GetAllAsync()
    {
        return await _dbContext.Drivers
            .Include(d => d.User)
            .Include(d => d.Employee)
            .ToListAsync();
    }

    public async Task<List<Driver>> GetActiveAsync()
    {
        return await _dbContext.Drivers
            .Include(d => d.User)
            .Include(d => d.Employee)
            .Where(d => d.IsActive && d.Employee.IsActive)
            .ToListAsync();
    }

    public async Task<List<Driver>> GetEligibleDriversAsync(DateTime start, DateTime end)
    {
        // For now, return all active drivers. Filtering by availability happens in manager or via separate call.
        // User requirements say "fetch eligible drivers using repository methods" but also "Satisfy driver availability windows".
        // The implementation plan splits this: Get eligible (active) drivers, then filter by availability.
        // However, the interface suggests filtering by start/end here?
        // User spec: "Task<List<Driver>> GetEligibleDriversAsync(DateTime start, DateTime end); - Returns active drivers (and optionally include Rating)"
        // It doesn't explicitly say "filter by availability" INSIDE this method, but the signature implies it might be relevant?
        // But simpler to return active drivers here (maybe pre-filtered if possible, but availability is complex).
        // I will return *active* drivers (same as GetActiveAsync essentially but maybe optimized or ready for future).
        // Actually, let's just use GetActiveAsync logic but ensure we validly implement the interface method.
        return await _dbContext.Drivers
            .Include(d => d.User)
            .Include(d => d.Employee)
            .Where(d => d.IsActive && d.Employee.IsActive)
            .ToListAsync();
    }

    public async Task<bool> DriverCodeExistsAsync(string driverCode)
    {
        return await _dbContext.Drivers.AnyAsync(d => d.DriverCode == driverCode);
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
