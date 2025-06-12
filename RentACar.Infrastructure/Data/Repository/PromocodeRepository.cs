using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Core.Repositories.Base;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repository
{
    class PromocodeRepository : Repository<Category>, IPromocodeRepository
    {
        private RentACarDbContext _dbContext;
        public PromocodeRepository(RentACarDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Promocode?> GetByNameAsync(string name)
        {
            return await _dbContext.Promocodes.FirstOrDefaultAsync(c => c.Name == name);
        }
        public async Task<List<Promocode>> GetActiveAsync()
        {
            return await _dbContext.Promocodes.Where(p => p.IsActive).ToListAsync();
        }

        // Implementations for the methods required by IRepository<Promocode>

        public async Task<IReadOnlyList<Promocode>> GetAllAsync()
        {
            return await _dbContext.Promocodes.ToListAsync();
        }

        public async Task<IReadOnlyList<Promocode>> GetAsync(Expression<Func<Promocode, bool>> predicate)
        {
            return await _dbContext.Promocodes.Where(predicate).ToListAsync();
        }

        public async Task<IReadOnlyList<Promocode>> GetAsync(Expression<Func<Promocode, bool>> predicate = null, Func<IQueryable<Promocode>, IOrderedQueryable<Promocode>> orderBy = null, string includeString = null, bool disableTracking = true)
        {
            IQueryable<Promocode> query = _dbContext.Promocodes;
            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (!string.IsNullOrWhiteSpace(includeString))
            {
                query = query.Include(includeString);
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<Promocode>> GetAsync(Expression<Func<Promocode, bool>> predicate = null, Func<IQueryable<Promocode>, IOrderedQueryable<Promocode>> orderBy = null, List<Expression<Func<Promocode, object>>> includes = null, bool disableTracking = true)
        {
            IQueryable<Promocode> query = _dbContext.Promocodes;
            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }

            return await query.ToListAsync();
        }

        public async Task<Promocode?> GetByIdAsync(int id)
        {
            return await _dbContext.Promocodes.FindAsync(id);
        }

        public async Task<Promocode> AddAsync(Promocode entity)
        {
            await _dbContext.Promocodes.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(Promocode entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Promocode entity)
        {
            _dbContext.Promocodes.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<Promocode?> GetByCodeAsync(string code)
        {
            return await _dbContext.Promocodes.FirstOrDefaultAsync(p => p.Name == code);
        }
    }
}
