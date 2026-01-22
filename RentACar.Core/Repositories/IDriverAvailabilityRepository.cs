using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;

namespace RentACar.Core.Repositories;

public interface IDriverAvailabilityRepository : IRepository<DriverAvailability>
{
    Task<List<DriverAvailability>> GetByDriverIdAsync(int driverId);
}
