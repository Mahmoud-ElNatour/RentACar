using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;

namespace RentACar.Core.Repositories
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetByIdsAsync(List<int> ids);

        Task<Customer?> GetByIdAsync(String id);
        Task<Customer?> GetByIdAsync(int id);
        Task<List<Customer>> GetAllAsync();
        Task<List<Customer>> FindByNameAsync(string name);
        IQueryable<Customer> Query();
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int id);
    }
}