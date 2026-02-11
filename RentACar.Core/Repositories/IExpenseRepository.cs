using System.Linq;
using System.Threading.Tasks;
using RentACar.Core.Entities;
using RentACar.Core.Repositories.Base;

namespace RentACar.Core.Repositories;

public interface IExpenseRepository : IRepository<Expense>
{
    IQueryable<Expense> QueryWithCategory();
    Task<Expense?> GetByIdWithCategoryAsync(int id);
}
