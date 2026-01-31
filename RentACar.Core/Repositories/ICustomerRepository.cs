using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;

namespace RentACar.Core.Repositories
{
    public interface ICustomerRepository
    {
<<<<<<< HEAD
=======
        Task<List<Customer>> GetByIdsAsync(List<int> ids);

>>>>>>> Mahmoud-V3
        Task<Customer?> GetByIdAsync(String id);
        Task<Customer?> GetByIdAsync(int id);
        Task<List<Customer>> GetAllAsync();
        Task<List<Customer>> FindByNameAsync(string name);
<<<<<<< HEAD
        IQueryable<Customer> Query();
=======
>>>>>>> Mahmoud-V3
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int id);
    }
}