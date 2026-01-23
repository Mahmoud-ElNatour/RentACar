using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;

namespace RentACar.Core.Repositories;

public interface IDriverAvailabilityRepository
{
    Task<List<DriverAvailability>> GetByDriverIdAsync(int driverId);
    Task AddAsync(DriverAvailability availability);
}
