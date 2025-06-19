using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data;

namespace RentACar.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly RentACarDbContext _dbContext; // Replace with your actual DbContext

        public CustomerRepository(RentACarDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _dbContext.Set<Customer>()
                                   .Include(c => c.User)
                                   .FirstOrDefaultAsync(c => c.UserId == id);
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _dbContext.Set<Customer>()
                                   .Include(c => c.User)
                                   .ToListAsync();
        }

        public async Task<List<Customer>> FindByNameAsync(string name)
        {
            return await _dbContext.Set<Customer>()
                                   .Include(c => c.User)
                                   .Where(c => c.Name.Contains(name))
                                   .ToListAsync();
        }

        public async Task AddAsync(Customer customer)
        {
            await _dbContext.Set<Customer>().AddAsync(customer);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer customer)
        {
            _dbContext.Entry(customer).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _dbContext.Set<Customer>().FindAsync(id);
            if (customer != null)
            {
                _dbContext.Set<Customer>().Remove(customer);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Customer?> GetByIdAsync(string aspNetUserId)
        {
            var customer = await _dbContext.Set<Customer>()
                                           .Include(c => c.User)
                                           .FirstOrDefaultAsync(c => c.aspNetUserId == aspNetUserId);
            return customer;
        }


        public async Task DeleteAsync(String aspNetUserId)
        {
            var customer = await _dbContext.Set<Customer>()
                                           .FirstOrDefaultAsync(c => c.aspNetUserId == aspNetUserId);
            if (customer != null)
            {
                _dbContext.Set<Customer>().Remove(customer);
                await _dbContext.SaveChangesAsync();
            }

        }
    }
}