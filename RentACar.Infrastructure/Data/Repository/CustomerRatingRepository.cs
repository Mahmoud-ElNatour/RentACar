using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data;

namespace RentACar.Infrastructure.Data.Repository
{

    public class CustomerRatingRepository : ICustomerRatingRepository
    {
        private readonly RentACarDbContext _dbContext;

        public CustomerRatingRepository(RentACarDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CustomerRating?> GetByIdAsync(int ratingId)
        {
            return await _dbContext.Set<CustomerRating>()
                                   .Include(r => r.Customer)
                                        .ThenInclude(c => c.User)
                                   .Include(r => r.Booking)
                                        .ThenInclude(b => b.Employeebooker)
                                            .ThenInclude(e => e.User)
                                   .Include(r => r.Booking)
                                        .ThenInclude(b => b.Car)
                                   .Include(r => r.Booking)
                                        .ThenInclude(b => b.Promocode)
                                   .Include(r => r.Booking)
                                        .ThenInclude(b => b.Payment)
                                   .FirstOrDefaultAsync(r => r.RatingId == ratingId);
        }

        public async Task<List<CustomerRating>> GetAllAsync(string? searchTerm = null, string? sortColumn = null, string? sortDirection = null)
        {
            var query = _dbContext.Set<CustomerRating>()
                                   .Include(r => r.Customer)
                                        .ThenInclude(c => c.User)
                                   .Include(r => r.Booking)
                                        .ThenInclude(b => b.Employeebooker)
                                            .ThenInclude(e => e.User)
                                   .Include(r => r.Booking)
                                        .ThenInclude(b => b.Payment)
                                   .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(r => 
                    r.Customer.User.UserName.ToLower().Contains(searchTerm) || 
                    (r.Customer.Name != null && r.Customer.Name.ToLower().Contains(searchTerm)) ||
                    r.BookingId.ToString().Contains(searchTerm) ||
                    (r.Feedback != null && r.Feedback.ToLower().Contains(searchTerm)));
            }

            // Default sorting
            if (string.IsNullOrEmpty(sortColumn))
            {
                sortColumn = "RatingDate";
                sortDirection = "desc";
            }

            bool isAscending = sortDirection?.ToLower() == "asc";

            query = sortColumn.ToLower() switch
            {
                "bookingid" => isAscending ? query.OrderBy(r => r.BookingId) : query.OrderByDescending(r => r.BookingId),
                "ratingdate" => isAscending ? query.OrderBy(r => r.RatingDate) : query.OrderByDescending(r => r.RatingDate),
                "stars" => isAscending ? query.OrderBy(r => r.Stars) : query.OrderByDescending(r => r.Stars),
                "customer" => isAscending ? query.OrderBy(r => r.Customer.Name) : query.OrderByDescending(r => r.Customer.Name),
                _ => query.OrderByDescending(r => r.RatingDate)
            };

            return await query.ToListAsync();
        }

        public async Task<List<CustomerRating>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _dbContext.Set<CustomerRating>()
                                   .Include(r => r.Customer)
                                   .Include(r => r.Booking)
                                   .Where(r => r.Booking.EmployeebookerId == employeeId)
                                   .OrderByDescending(r => r.RatingDate)
                                   .ToListAsync();
        }

        public async Task<List<CustomerRating>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbContext.Set<CustomerRating>()
                                   .Include(r => r.Booking)
                                        .ThenInclude(b => b.Employeebooker)
                                   .Where(r => r.CustomerId == customerId)
                                   .OrderByDescending(r => r.RatingDate)
                                   .ToListAsync();
        }

        public async Task AddAsync(CustomerRating rating)
        {
            await _dbContext.Set<CustomerRating>().AddAsync(rating);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int ratingId)
        {
            var rating = await _dbContext.Set<CustomerRating>().FindAsync(ratingId);
            if (rating != null)
            {
                _dbContext.Set<CustomerRating>().Remove(rating);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<CustomerRating?> GetByBookingIdAsync(int bookingId)
        {
            return await _dbContext.Set<CustomerRating>()
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync(r => r.BookingId == bookingId);
        }
    }
}
