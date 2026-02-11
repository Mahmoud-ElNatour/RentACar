using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;
using RentACar.Core.Repositories.Base;

namespace RentACar.Core.Repositories;

public interface IExpenseCategoryRepository : IRepository<ExpenseCategory>
{
    Task<List<ExpenseCategory>> GetActiveAsync();
    Task<ExpenseCategory?> GetByIdAsync(int id);
    Task<bool> ExistsByNameAsync(string name);
}
