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
    Task<List<Driver>> GetEligibleDriversAsync(DateTime start, DateTime end);
    Task<List<Driver>> GetAvailableDriversForBookingAsync(DateOnly start, DateOnly end, int categoryId);

    Task AddAsync(Driver driver);
    Task UpdateAsync(Driver driver);
    Task DeleteAsync(int id);
}
