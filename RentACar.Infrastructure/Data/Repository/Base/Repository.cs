﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Repositories.Base;

namespace RentACar.Infrastructure.Data.Repository.Base
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly RentACarDbContext _dbContext;

        public Repository(RentACarDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>().Where(predicate).ToListAsync();
        }

        // Note sure aboute those, but when I remove it I get an error!!!
        public Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeString = null, bool disableTracking = true)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, List<Expression<Func<T, object>>> includes = null, bool disableTracking = true)
        {
            throw new NotImplementedException();
        }

        //public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeString = null, bool disableTracking = true)
        //{
        //    IQueryable<T> query = _dbContext.Set<T>();
        //    if (disableTracking) query = query.AsNoTracking();

        //    if (!string.IsNullOrWhiteSpace(includeString)) query = query.Include(includeString);

        //    if (predicate != null) query = query.Where(predicate);

        //    if (orderBy != null)
        //        return await orderBy(query).ToListAsync();
        //    return await query.ToListAsync();
        //}

        //public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, List<Expression<Func<T, object>>> includes = null, bool disableTracking = true)
        //{
        //    IQueryable<T> query = _dbContext.Set<T>();
        //    if (disableTracking) query = query.AsNoTracking();

        //    if (includes != null) query = includes.Aggregate(query, (current, include) => current.Include(include));

        //    if (predicate != null) query = query.Where(predicate);

        //    if (orderBy != null)
        //        return await orderBy(query).ToListAsync();
        //    return await query.ToListAsync();
        //}

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<T> AddAsync(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }


    }
}
