using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repository;

public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    private readonly RentACarDbContext _dbContext;

    public ExpenseRepository(RentACarDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<Expense> QueryWithCategory()
    {
        return _dbContext.Expenses
            .Include(x => x.ExpenseCategory)
            .Include(x => x.CreatedByUser)
            .OrderByDescending(x => x.ExpenseDate);
    }

    public async Task<Expense?> GetByIdWithCategoryAsync(int id)
    {
        return await _dbContext.Expenses
            .Include(x => x.ExpenseCategory)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.ExpenseId == id);
    }
}
