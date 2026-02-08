using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Infrastructure.Data.Repository
{
    public class DriverAvailabilityRepository : IDriverAvailabilityRepository
    {
        private readonly RentACarDbContext _dbContext;
        private readonly Microsoft.Extensions.Logging.ILogger<DriverAvailabilityRepository> _logger;

        public DriverAvailabilityRepository(RentACarDbContext dbContext, Microsoft.Extensions.Logging.ILogger<DriverAvailabilityRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<DriverAvailability>> GetByDriverIdAsync(int driverId)
        {
            return await _dbContext.DriverAvailabilities
                .Where(a => a.DriverId == driverId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<List<DriverAvailability>> GetByDriverIdAndRangeAsync(int driverId, DateOnly from, DateOnly to)
        {
            return await _dbContext.DriverAvailabilities
                .Where(a => a.DriverId == driverId && a.Date >= from && a.Date <= to)
                .OrderBy(a => a.Date)
                .ToListAsync();
        }

        public async Task<List<DriverAvailability>> GetAvailabilitiesForDriversAsync(List<int> driverIds, DateOnly from, DateOnly to)
        {
            if (driverIds == null || !driverIds.Any())
                return new List<DriverAvailability>();

            return await _dbContext.DriverAvailabilities
                .Where(a => driverIds.Contains(a.DriverId) && a.IsAvailable && a.Date >= from && a.Date <= to)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// FULL-DAY (Option 1):
        /// Driver is eligible only if ALL days in [startDate..endDate] exist with IsAvailable = true.
        /// Times (StartTime/EndTime) are ignored completely.
        /// </summary>
        public async Task<bool> HasCoveringAvailabilityAsync(int driverId, DateTime start, DateTime end)
        {
            // Normalize: booking logic passes start at 00:00 and end at 23:59 (or similar).
            // We only care about dates.
            var startDate = DateOnly.FromDateTime(start);
            var endDate = DateOnly.FromDateTime(end);

            if (endDate < startDate) return false;

            // required days inclusive
            var requiredDays = (endDate.DayNumber - startDate.DayNumber) + 1;

            _logger.LogInformation("Checking availability for Driver {DriverId}: Range {Start} to {End} ({Required} days)", driverId, startDate, endDate, requiredDays);

            // Count how many available rows exist in the range
            var availableDays = await _dbContext.DriverAvailabilities
                .Where(a => a.DriverId == driverId)
                .Where(a => a.IsAvailable)
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .Select(a => a.Date)        // protect against duplicates (until unique index exists)
                .Distinct()
                .CountAsync();

            _logger.LogInformation("Found {Found} available days for Driver {DriverId}. Match: {Match}", availableDays, driverId, availableDays == requiredDays);

            return availableDays == requiredDays;
        }

        public async Task UpsertRangeAsync(int driverId, DateOnly from, DateOnly to, bool isAvailable)
        {
            if (from > to) (from, to) = (to, from); // Ensure correct order

            // 1. Fetch existing rows in this range
            var existingRows = await _dbContext.DriverAvailabilities
                .Where(a => a.DriverId == driverId && a.Date >= from && a.Date <= to)
                .ToListAsync();

            var existingMap = existingRows.ToDictionary(r => r.Date);

            var now = DateTime.UtcNow;
            var newRows = new List<DriverAvailability>();

            // 2. Iterate through every day in range
            for (var dt = from; dt <= to; dt = dt.AddDays(1))
            {
                // Full-Day Logic enforcement
                TimeOnly? sTime = isAvailable ? new TimeOnly(0, 0) : null;
                TimeOnly? eTime = isAvailable ? new TimeOnly(23, 59) : null;

                if (existingMap.TryGetValue(dt, out var row))
                {
                    // Update existing
                    row.IsAvailable = isAvailable;
                    row.StartTime = sTime;
                    row.EndTime = eTime;
                    row.UpdatedAt = now;
                    // StartDateTime/EndDateTime legacy fields - kept null or could specific if needed.
                    // Ideally keep them ignored as we moved to Option 1.
                }
                else
                {
                    // Create new
                    newRows.Add(new DriverAvailability
                    {
                        DriverId = driverId,
                        Date = dt,
                        IsAvailable = isAvailable,
                        StartTime = sTime,
                        EndTime = eTime,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
            }

            if (newRows.Any())
            {
                await _dbContext.DriverAvailabilities.AddRangeAsync(newRows);
            }

            // 3. Save Changes
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddAsync(DriverAvailability availability)
        {
            await _dbContext.DriverAvailabilities.AddAsync(availability);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(DriverAvailability availability)
        {
            _dbContext.DriverAvailabilities.Update(availability);
            await _dbContext.SaveChangesAsync();
        }
    }
}
