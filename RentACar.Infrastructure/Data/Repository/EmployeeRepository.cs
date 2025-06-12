using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data;

namespace RentACar.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly RentACarDbContext _dbContext; // Replace with your actual DbContext

        public EmployeeRepository(RentACarDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _dbContext.Set<Employee>().FindAsync(id);
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _dbContext.Set<Employee>().ToListAsync();
        }

        public async Task AddAsync(Employee employee)
        {
            await _dbContext.Set<Employee>().AddAsync(employee);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            _dbContext.Entry(employee).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var employee = await _dbContext.Set<Employee>().FindAsync(id);
            if (employee != null)
            {
                _dbContext.Set<Employee>().Remove(employee);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Employee?> GetByIdAsync(string aspNetUserId)
        {
            var Employee = await _dbContext.Set<Employee>()
                                           .FirstOrDefaultAsync(c => c.aspNetUserId == aspNetUserId);
            return Employee;
        }
    }
}