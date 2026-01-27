using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;

namespace RentACar.Core.Repositories
{
    public interface ICustomerRatingRepository
    {
        Task<CustomerRating?> GetByIdAsync(int ratingId);
        Task<List<CustomerRating>> GetByEmployeeIdAsync(int employeeId);
        Task<List<CustomerRating>> GetByCustomerIdAsync(int customerId);
        Task AddAsync(CustomerRating rating);
        Task DeleteAsync(int ratingId);
    }
}

