using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;

namespace RentACar.Core.Repositories;

public interface IDriverLocationRepository : IRepository<DriverLocation>
{
    Task<DriverLocation?> GetLatestByBookingIdAsync(int bookingId);
    Task<List<DriverLocation>> GetByDriverIdAsync(int driverId);
}
