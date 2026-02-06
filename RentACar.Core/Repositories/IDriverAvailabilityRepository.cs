using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;

namespace RentACar.Core.Repositories;

public interface IDriverAvailabilityRepository
{
    Task<List<DriverAvailability>> GetByDriverIdAsync(int driverId);
    Task<List<DriverAvailability>> GetByDriverIdAndRangeAsync(int driverId, DateOnly from, DateOnly to);

    // Full rental period coverage (daily)
    Task<bool> HasCoveringAvailabilityAsync(int driverId, DateTime start, DateTime end);

    Task UpsertRangeAsync(int driverId, DateOnly from, DateOnly to, bool isAvailable);

    Task AddAsync(DriverAvailability availability);
    Task UpdateAsync(DriverAvailability availability);
}
