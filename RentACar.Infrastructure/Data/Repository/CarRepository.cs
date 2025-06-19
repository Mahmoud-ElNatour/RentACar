// review later GetAvailabilityInTimelineAsyn

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repositories
{
    public class CarRepository : Repository<Car>, ICarRepository
    {
        private new readonly RentACarDbContext _dbContext; // Adjust DbContext type if needed

        public CarRepository(RentACarDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Car?> GetByPlateNumberAsync(string plateNumber)
        {
            return await _dbContext.Cars.FirstOrDefaultAsync(c => c.PlateNumber == plateNumber);
        }

        public async Task<List<Car>> GetByCategoryAsync(int categoryId)
        {
            return await _dbContext.Cars.Where(c => c.CategoryId == categoryId).ToListAsync();
        }

        public async Task<List<Car>> GetByModelAsync(string modelName)
        {
            return await _dbContext.Cars.Where(c => c.ModelName.Contains(modelName)).ToListAsync();
        }

        public async Task<List<Car>> GetByYearAsync(int modelYear)
        {
            return await _dbContext.Cars.Where(c => c.ModelYear == modelYear).ToListAsync();
        }



        public async Task<List<Car>> SearchByFilterAsync(string? modelName = null, int? modelYear = null, int? categoryId = null, bool? isAvailable = null)
        {
            var query = _dbContext.Cars
                .Include(c => c.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(modelName))
            {
                query = query.Where(c => c.ModelName.Contains(modelName));
            }

            if (modelYear.HasValue)
            {
                query = query.Where(c => c.ModelYear == modelYear.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }

            if (isAvailable.HasValue)
            {
                query = query.Where(c => c.IsAvailable == isAvailable.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<List<Car>> BrowseAllCarsAsync()
        {
            return await _dbContext.Cars
                  .Include(c => c.Category)
                  .ToListAsync();
        }

        public async Task UpdateCarAvailabilityAsync(int carId, bool isAvailable)
        {
            var car = await _dbContext.Cars.FindAsync(carId);
            if (car != null)
            {
                car.IsAvailable = isAvailable;
                _dbContext.Update(car);
                await _dbContext.SaveChangesAsync();
            }
        }

        public Task DeleteAsync(int id)
        {
            var car = _dbContext.Cars.Find(id);
            if (car != null)
            {
                _dbContext.Cars.Remove(car);
                return _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Car not found");
            }
        }

        public async Task<List<Car>> GetAvailabilityInTimelineAsync(DateTime startTime, DateTime endTime)
        {
            var start = DateOnly.FromDateTime(startTime.Date);
            var end = DateOnly.FromDateTime(endTime.Date);

            return await _dbContext.Cars
                .Include(c => c.Category)
                .Where(car => !_dbContext.Bookings.Any(b => b.CarId == car.CarId && b.Startdate <= end && b.Enddate >= start))
                .ToListAsync();
        }

        public Task SetCarAvailabilityAsync(int carId, bool isAvailable)
        {
            var car = _dbContext.Cars.Find(carId);
            if (car != null)
            {
                car.IsAvailable = isAvailable;
                _dbContext.Update(car);
                return _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Car not found");
            }
        }
    }
}