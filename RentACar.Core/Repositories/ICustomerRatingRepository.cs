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
        Task<List<CustomerRating>> GetAllAsync(string? searchTerm = null, string? sortColumn = null, string? sortDirection = null);
        Task AddAsync(CustomerRating rating);
        Task<CustomerRating?> GetByBookingIdAsync(int bookingId);
        Task DeleteAsync(int ratingId);
    }
}

