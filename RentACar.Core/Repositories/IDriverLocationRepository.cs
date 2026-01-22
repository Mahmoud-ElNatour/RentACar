using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;
using RentACar.Core.Repositories.Base;

namespace RentACar.Core.Repositories;

public interface IDriverLocationRepository : IRepository<DriverLocation>
{
    Task<DriverLocation?> GetLatestByBookingIdAsync(int bookingId);
    Task<List<DriverLocation>> GetByDriverIdAsync(int driverId);
}
