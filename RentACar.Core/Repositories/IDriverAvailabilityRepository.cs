using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;

namespace RentACar.Core.Repositories;

public interface IDriverAvailabilityRepository
{
    Task<List<DriverAvailability>> GetByDriverIdAsync(int driverId);
    Task<List<DriverAvailability>> GetByDriverIdAndRangeAsync(int driverId, DateOnly from, DateOnly to);
    Task<bool> HasCoveringAvailabilityAsync(int driverId, DateTime start, DateTime end);
    Task AddAsync(DriverAvailability availability);
    Task UpdateAsync(DriverAvailability availability);
}
