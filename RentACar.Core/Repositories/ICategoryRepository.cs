using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RentACar.Core.Entities;
using RentACar.Core.Repositories.Base;

namespace RentACar.Core.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task DeleteAsync(int id);
        Task<Category?> GetByNameAsync(string name);
        Task<Category?> UpdateCategoryNameAsync(int id, string newName);
        Task<Category?> DeleteCategoryByNameAsync(string name, string userId);
    }
}
