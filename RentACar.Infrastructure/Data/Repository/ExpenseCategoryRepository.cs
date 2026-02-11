using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repository;

public class ExpenseCategoryRepository : Repository<ExpenseCategory>, IExpenseCategoryRepository
{
    private readonly RentACarDbContext _dbContext;

    public ExpenseCategoryRepository(RentACarDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ExpenseCategory>> GetActiveAsync()
    {
        return await _dbContext.ExpenseCategories
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<ExpenseCategory?> GetByIdAsync(int id)
    {
        return await _dbContext.ExpenseCategories
            .Include(x => x.Expenses)
            .FirstOrDefaultAsync(x => x.ExpenseCategoryId == id);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbContext.ExpenseCategories.AnyAsync(x => x.Name == name);
    }
}
