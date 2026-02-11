using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly RentACarDbContext _dbContext; // Adjust DbContext type if needed

        public BookingRepository(RentACarDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;

        }

        public async Task<List<Booking>> GetBookingsByCustomerIdAsync(int customerId)
        {
            return await _dbContext.Bookings
                .Include(b => b.Car)
                .Where(b => b.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByCarIdAsync(int carId)
        {
            return await _dbContext.Bookings.Where(b => b.CarId == carId).ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsBetweenDatesAsync(DateOnly startDate, DateOnly endDate)
        {
            return await _dbContext.Bookings
                .Where(b => b.Startdate <= endDate && b.Enddate >= startDate)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _dbContext.Bookings
                .Include(b => b.Customer)
                    .ThenInclude(c => c.User)
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task<List<Booking>> GetBookingsByEmployeeIdAsync(int employeeId)
        {
            return await _dbContext.Bookings.Where(b => b.EmployeebookerId == employeeId).ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByDriverIdAsync(int driverId)
        {
            return await _dbContext.Bookings
                .Include(b => b.Customer)
                    .ThenInclude(c => c.User)
                .Include(b => b.Car)
                .Where(b => b.DriverId == driverId)
                .ToListAsync();
        }
        public async Task UpdateCarAvailabilityAsync(int carId, bool isAvailable)
        {
            var car = await _dbContext.Cars.FindAsync(carId);
            if (car != null)
            {
                car.IsAvailable = isAvailable;
                _dbContext.Update(car);
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task<List<int>> GetConflictingDriverIdsAsync(DateOnly start, DateOnly end)
        {
            // Identify drivers who have *blocking* bookings overlapping the requested range.
            // Status logic should match 'IsBlockingStatus' from BookingManager (duplicated here or reused if possible, but repo shouldn't depend on manager).
            // We'll reimplement the blocking check logic in LINQ/SQL.
            // Blocking: !Completed && !Returned && !Rejected && !Cancelled

            var conflictDriverIds = await _dbContext.Bookings
                .Where(b => b.HasDriver && b.DriverId != null) // Only driver bookings
                .Where(b => b.Startdate <= end && b.Enddate >= start) // Overlap check
                .Where(b => b.BookingStatus != "Completed"
                            && b.BookingStatus != "Returned"
                            && b.BookingStatus != "Rejected"
                            && b.BookingStatus != "Cancelled")
                .Select(b => b.DriverId.Value)
                .Distinct()
                .ToListAsync();

            return conflictDriverIds;
        }

        public async Task UpdateAsync(Booking booking)
        {
            _dbContext.Entry(booking).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
    }
}
