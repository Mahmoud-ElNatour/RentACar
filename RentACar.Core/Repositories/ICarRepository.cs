using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RentACar.Core.Entities;
using RentACar.Core.Repositories.Base;
using RentACar.Core.Enums;

namespace RentACar.Core.Repositories
{
    public interface ICarRepository : IRepository<Car>
    {
        //Task<Car> GetCarByIdAsync(int id);

        Task<Car?> GetByPlateNumberAsync(string plateNumber);
        Task<List<Car>> GetByCategoryAsync(int categoryId);
        Task<List<Car>> GetByModelAsync(string modelName);
        Task<List<Car>> GetByYearAsync(int modelYear);
        Task<List<Car>> GetAvailabilityInTimelineAsync(DateTime startTime, DateTime endTime);
        Task<List<Car>> SearchByFilterAsync(
            string? modelName = null, 
            int? modelYear = null, 
            int? categoryId = null, 
            bool? isAvailable = null,
            TransmissionType[]? transmissionTypes = null,
            FuelType[]? fuelTypes = null,
            int? minSeats = null,
            int? minLuggage = null,
            bool? hasInfotainment = null,
            bool? hasGPS = null,
            bool? hasSunroof = null,
            bool? hasParkingSensors = null,
            bool? hasRearCamera = null);
        Task<List<Car>> BrowseAllCarsAsync();
        Task UpdateCarAvailabilityAsync(int carId, bool isAvailable);
        Task DeleteAsync(int id);
        Task SetCarAvailabilityAsync(int carId, bool isAvailable);

    }
}
