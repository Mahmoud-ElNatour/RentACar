using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Infrastructure.Data.Repository;

public class TravelActionLogRepository : ITravelActionLogRepository
{
    private readonly RentACarDbContext _dbContext;

    public TravelActionLogRepository(RentACarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TravelActionLog> AddAsync(TravelActionLog log)
    {
        await _dbContext.TravelActionLogs.AddAsync(log);
        await _dbContext.SaveChangesAsync();
        return log;
    }

    public async Task<List<TravelActionLog>> GetRecentAsync(int limit = 100)
    {
        return await _dbContext.TravelActionLogs
            .Include(l => l.Customer)
            .OrderByDescending(l => l.CreatedAtUtc)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<TravelActionLog>> GetByCustomerUsernameAsync(string customerUsername, int limit = 100)
    {
        return await _dbContext.TravelActionLogs
            .Include(l => l.Customer)
            .Where(l => l.CustomerUsername == customerUsername)
            .OrderByDescending(l => l.CreatedAtUtc)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<TravelActionLog>> GetByDateRangeAsync(DateTime fromUtc, DateTime toUtc, int limit = 200)
    {
        return await _dbContext.TravelActionLogs
            .Include(l => l.Customer)
            .Where(l => l.CreatedAtUtc >= fromUtc && l.CreatedAtUtc <= toUtc)
            .OrderByDescending(l => l.CreatedAtUtc)
            .Take(limit)
            .ToListAsync();
    }
}
