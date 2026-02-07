using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Infrastructure.Data.Repository;

public class TripRepository : ITripRepository
{
    private readonly RentACarDbContext _dbContext;

    public TripRepository(RentACarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Trip?> GetTripByIdAsync(int tripId)
    {
        return await _dbContext.Trips
            .Include(t => t.Booking)
            .Include(t => t.Driver)
            .FirstOrDefaultAsync(t => t.TripId == tripId);
    }

    public async Task<Trip?> GetTripByBookingIdAsync(int bookingId)
    {
        return await _dbContext.Trips
            .Include(t => t.Booking)
            .Include(t => t.Driver)
            .FirstOrDefaultAsync(t => t.BookingId == bookingId);
    }

    public async Task<List<Trip>> GetTripsByDriverIdAsync(int driverId)
    {
        return await _dbContext.Trips
            .AsNoTracking()
            .Include(t => t.Booking)
            .Include(t => t.Driver)
            .Where(t => t.DriverId == driverId)
            .ToListAsync();
    }

    public async Task<Trip> CreateTripAsync(Trip trip)
    {
        _dbContext.Trips.Add(trip);
        await _dbContext.SaveChangesAsync();
        return trip;
    }

    public async Task UpdateTripAsync(Trip trip)
    {
        _dbContext.Entry(trip).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<int>> GetActiveBookingIdsForDriverAsync(int driverId)
    {
        var activeStatuses = new[] {
            TripStatus.Pending,
            TripStatus.Assigned,
            TripStatus.OnTheWay,
            TripStatus.Arrived,
            TripStatus.InTrip
        };

        return await _dbContext.Trips
            .AsNoTracking()
            .Where(t => t.DriverId == driverId && activeStatuses.Contains(t.TripStatus))
            .Select(t => t.BookingId)
            .ToListAsync();
    }
}
