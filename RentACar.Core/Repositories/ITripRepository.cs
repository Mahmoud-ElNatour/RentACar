using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;

namespace RentACar.Core.Repositories;

public interface ITripRepository
{
    Task<Trip?> GetTripByIdAsync(int tripId);
    Task<Trip?> GetTripByBookingIdAsync(int bookingId);
    Task<List<Trip>> GetTripsByDriverIdAsync(int driverId);
    Task<Trip> CreateTripAsync(Trip trip);
    Task UpdateTripAsync(Trip trip);
    Task<List<int>> GetActiveBookingIdsForDriverAsync(int driverId);
}
