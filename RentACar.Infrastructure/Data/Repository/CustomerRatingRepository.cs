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
                                   .Include(r => r.Booking)
                                        .ThenInclude(b => b.Employeebooker)
                                   .FirstOrDefaultAsync(r => r.RatingId == ratingId);
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
    }
}
