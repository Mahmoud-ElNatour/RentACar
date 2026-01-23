using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;

namespace RentACar.Core.Repositories;

public interface IDriverRepository
{
    Task<Driver?> GetByIdAsync(int id);
    Task<Driver?> GetByEmployeeIdAsync(int employeeId);
    Task<Driver?> GetByAspNetUserIdAsync(string aspNetUserId);
    Task<List<Driver>> GetAllAsync();
    Task<List<Driver>> GetActiveAsync();
    Task<bool> DriverCodeExistsAsync(string driverCode);
    Task AddAsync(Driver driver);
    Task UpdateAsync(Driver driver);
    Task DeleteAsync(int id);
}
