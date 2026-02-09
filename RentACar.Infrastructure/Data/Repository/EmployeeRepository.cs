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
            return await _dbContext.Set<Employee>()
                                   .Include(e => e.User)
                                   .Include(e => e.BlackLists)
                                   .Include(e => e.Driver)
                                   .FirstOrDefaultAsync(e => e.EmployeeId == id);
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _dbContext.Set<Employee>()
                                   .Include(e => e.User)
                                   .Include(e => e.BlackLists)
                                   .Include(e => e.Driver)
                                   .ToListAsync();
        }

        public async Task AddAsync(Employee employee)
        {
            await _dbContext.Set<Employee>().AddAsync(employee);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            if (_dbContext.Entry(employee).State == EntityState.Detached)
            {
                _dbContext.Employees.Attach(employee);
            }
            _dbContext.Entry(employee).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var employee = await _dbContext.Employees.FindAsync(id);
            if (employee != null)
            {
                _dbContext.Employees.Remove(employee);
                await _dbContext.SaveChangesAsync();
            }
        }

        public IQueryable<Employee> Query()
        {
            return _dbContext.Employees;
        }

        public async Task<Employee?> GetByIdAsync(string id)
        {
            return await _dbContext.Employees
                .Include(e => e.User)
                .Include(e => e.BlackLists)
                .Include(e => e.Driver)
                .FirstOrDefaultAsync(e => e.aspNetUserId == id);
        }
    }
}