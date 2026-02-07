using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Repositories;

namespace RentACar.Infrastructure.Data.Repository
{
    public class DriverAllowedCategoryRepository : IDriverAllowedCategoryRepository
    {
        private readonly RentACarDbContext _dbContext;

        public DriverAllowedCategoryRepository(RentACarDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<int>> GetAllowedCategoryIdsByDriverIdAsync(int driverId)
        {
            return await _dbContext.DriverAllowedCategories
                .Where(x => x.DriverId == driverId)
                .Select(x => x.CategoryId)
                .ToListAsync();
        }

        public async Task SetAllowedCategoriesAsync(int driverId, List<int> categoryIds)
        {
            var distinctIds = categoryIds.Distinct().ToList();

            var existing = await _dbContext.DriverAllowedCategories
                .Where(x => x.DriverId == driverId)
                .ToListAsync();

            var toRemove = existing.Where(x => !distinctIds.Contains(x.CategoryId)).ToList();
            if (toRemove.Any())
            {
                _dbContext.DriverAllowedCategories.RemoveRange(toRemove);
            }

            var existingCategoryIds = existing.Select(x => x.CategoryId).ToHashSet();
            var toAdd = distinctIds
                .Where(categoryId => !existingCategoryIds.Contains(categoryId))
                .Select(categoryId => new Core.Entities.DriverAllowedCategory
                {
                    DriverId = driverId,
                    CategoryId = categoryId
                })
                .ToList();

            if (toAdd.Any())
            {
                await _dbContext.DriverAllowedCategories.AddRangeAsync(toAdd);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> DriverCanDriveCategoryAsync(int driverId, int categoryId)
        {
            return await _dbContext.DriverAllowedCategories
                .AnyAsync(x => x.DriverId == driverId && x.CategoryId == categoryId);
        }
    }
}
