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
            return await _dbContext.Bookings.Where(b => b.CustomerId == customerId).ToListAsync();
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
            return await _dbContext.Bookings.FindAsync(id);
        }

        public async Task<List<Booking>> GetBookingsByEmployeeIdAsync(int employeeId)
        {
            return await _dbContext.Bookings.Where(b => b.EmployeebookerId == employeeId).ToListAsync();
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
        public async Task UpdateAsync(Booking booking)
        {
            _dbContext.Entry(booking).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            var booking = await _dbContext.Bookings.FindAsync(bookingId);
            if (booking == null)
                return false;

            _dbContext.Bookings.Remove(booking);
            await _dbContext.SaveChangesAsync();
            return true;
        }

    }
}
