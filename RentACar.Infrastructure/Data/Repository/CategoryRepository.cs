using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private  RentACarDbContext _dbContext;
        public CategoryRepository(RentACarDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Category>> GetAllAsync()
        {
            return await _dbContext.Categories.Include(c => c.Cars).ToListAsync();
        }

        public Task DeleteAsync(int id)
        {
            var category = _dbContext.Categories.Find(id);
            if (category != null)
            {
                _dbContext.Categories.Remove(category);
                return _dbContext.SaveChangesAsync();
            }
            return Task.CompletedTask;
        }

        public Task<Category?> DeleteCategoryByNameAsync(string name, string userId)
        {
            var category = _dbContext.Categories.FirstOrDefault(c => c.Name == name);
            if (category != null)
            {
                _dbContext.Categories.Remove(category);
                return _dbContext.SaveChangesAsync().ContinueWith(t => category);
            }
            return Task.FromResult<Category?>(null);
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == name);
        }


        public async Task<Category?> UpdateCategoryNameAsync(int id, string newName)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category != null)
            {
                category.Name = newName;
                await _dbContext.SaveChangesAsync();
                return category;
            }
            return null;
        }

        public async Task<bool> HasCarsAsync(int id)
        {
            return await _dbContext.Cars.AnyAsync(c => c.CategoryId == id);
        }

        public async Task DeactivateCarsAsync(int categoryId)
        {
            var cars = await _dbContext.Cars.Where(c => c.CategoryId == categoryId).ToListAsync();
            foreach (var car in cars)
            {
                car.IsAvailable = false;
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Category>> GetAllActiveAsync()
        {
            return await _dbContext.Categories.Where(c => c.IsActive).ToListAsync();
        }
    }
}
