using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AspNetUser = RentACar.Core.Entities.AspNetUser;

namespace RentACar.Application.Managers
{
    public class CarManager
    {
        private readonly ICarRepository _carRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager; // Inject UserManager for role checking
        private readonly ILogger<CarManager> _logger;
        private readonly AuditLogManager _auditLogManager;
        private readonly EmailManager _emailManager;
        private readonly EmployeeManager _employeeManager;

        public CarManager(ICarRepository carRepository, IMapper mapper, UserManager<IdentityUser> userManager, ILogger<CarManager> logger, AuditLogManager auditLogManager, EmailManager emailManager, EmployeeManager employeeManager)
        {
            _carRepository = carRepository;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
            _auditLogManager = auditLogManager;
            _emailManager = emailManager;
            _employeeManager = employeeManager;
        }

        public async Task<CarDto?> AddCarAsync(CarDto carDto, string userId)
        {
            _logger.LogInformation("Attempting to add car {@Car}", carDto);
            // 1. Check if the user has the "Admin" role
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                // Optionally throw an exception or return null/an error DTO
                // indicating unauthorized access.
                _logger.LogWarning("User {UserId} not authorized to add cars", userId);
                return null; // Or throw new UnauthorizedAccessException("Only admins can add cars.");
            }

            // 2. Check for unique plate number
            var existingCar = await _carRepository.GetByPlateNumberAsync(carDto.PlateNumber);
            if (existingCar != null)
            {
                // Optionally throw an exception or return null/an error DTO
                // indicating that the plate number already exists.
                _logger.LogWarning("Car with plate number {Plate} already exists", carDto.PlateNumber);
                return null; // Or throw new InvalidOperationException($"Car with plate number '{carDto.PlateNumber}' already exists.");
            }

            // 3. Map the DTO to the entity
            var carEntity = _mapper.Map<Car>(carDto);

            // 4. Add the entity to the repository
            await _carRepository.AddAsync(carEntity);
            _logger.LogInformation("Car added with id {Id}", carEntity.CarId);

            await _auditLogManager.LogAsync("Create", "Car", carEntity.CarId.ToString(), $"Added new car: {carEntity.ModelName} ({carEntity.ModelYear}) - {carEntity.PlateNumber}");

            // 5. Send Car Update Email (Create)
            var emails = await _employeeManager.GetActiveEmployeeEmailsAsync();
            await _emailManager.SendCarUpdateEmail(emails, carEntity, "Create", "New Car", "N/A", "Created", "System/Admin");

            // 6. Map the created entity back to a DTO and return it
            return _mapper.Map<CarDto>(carEntity);
        }

        public async Task<CarDto?> GetCarByIdAsync(int id)
        {
            _logger.LogInformation("Fetching car {Id}", id);
            var car = await _carRepository.GetByIdAsync(id);
            return _mapper.Map<CarDto>(car);
        }

        public async Task<List<CarDto>> GetCarsByCategoryAsync(int categoryId)
        {
            var cars = await _carRepository.GetByCategoryAsync(categoryId);
            return _mapper.Map<List<CarDto>>(cars);
        }

        public async Task<List<CarDto>> GetCarsByModelAsync(string modelName)
        {
            var cars = await _carRepository.GetByModelAsync(modelName);
            return _mapper.Map<List<CarDto>>(cars);
        }

        public async Task<List<CarDto>> GetCarsByYearAsync(int modelYear)
        {
            var cars = await _carRepository.GetByYearAsync(modelYear);
            return _mapper.Map<List<CarDto>>(cars);
        }

        public async Task<List<CarDto>> GetAvailableCarsInTimelineAsync(DateTime startTime, DateTime endTime)
        {
            var cars = await _carRepository.GetAvailabilityInTimelineAsync(startTime, endTime);
            return _mapper.Map<List<CarDto>>(cars);
        }

        public async Task<List<CarDto>> SearchCarsByFilterAsync(string? modelName = null, int? modelYear = null, int? categoryId = null, bool? isAvailable = null)
        {
            var cars = await _carRepository.SearchByFilterAsync(modelName, modelYear, categoryId, isAvailable);
            return _mapper.Map<List<CarDto>>(cars);
        }

        public async Task<List<CarDto>> BrowseAllCarsAsync()
        {
            var cars = await _carRepository.BrowseAllCarsAsync();
            return _mapper.Map<List<CarDto>>(cars);
        }

        public async Task UpdateCarAvailabilityAsync(int carId, bool isAvailable)
        {
            _logger.LogInformation("Updating availability for car {Id} to {Avail}", carId, isAvailable);
            await _carRepository.UpdateCarAvailabilityAsync(carId, isAvailable);
            await _auditLogManager.LogAsync("Update", "Car", carId.ToString(), $"Updated availability to: {isAvailable}");
            
            // 📨 Send Car Update Email
            var car = await _carRepository.GetByIdAsync(carId);
            if (car != null) {
                var emails = await _employeeManager.GetActiveEmployeeEmailsAsync();
                await _emailManager.SendCarUpdateEmail(emails, car, "Update", "IsAvailable", (!isAvailable).ToString(), isAvailable.ToString(), "System/Admin");
            }
        }

        public async Task UpdateCarAsync(CarDto carDto)
        {
            var existingCar = await _carRepository.GetByIdAsync(carDto.CarId);
            if (existingCar != null)
            {
                _logger.LogInformation("Updating car {Id}", carDto.CarId);
                
                var oldPrice = existingCar.PricePerDay;
                var oldModel = existingCar.ModelName;
                
                _mapper.Map(carDto, existingCar);
                await _carRepository.UpdateAsync(existingCar);
                await _auditLogManager.LogAsync("Update", "Car", carDto.CarId.ToString(), $"Updated car details: {carDto.ModelName} - {carDto.PlateNumber}");

                // 📨 Send Car Update Email (Price Change)
                if (oldPrice != existingCar.PricePerDay) {
                     var emails = await _employeeManager.GetActiveEmployeeEmailsAsync();
                     await _emailManager.SendCarUpdateEmail(emails, existingCar, "Update", "PricePerDay", oldPrice?.ToString() ?? "N/A", existingCar.PricePerDay?.ToString() ?? "N/A", "System/Admin");
                }
                // (Note: The spec asked for 'Car updates', triggering on Price/Availability/Delete. Is it only Price? "Car price changed")
                // Yes, "Car price changed", "Car availability changed", "Car deleted/archived".
            }
            else
            {
                _logger.LogWarning("Car {Id} not found for update", carDto.CarId);
                await _auditLogManager.LogAsync("Update", "Car", carDto.CarId.ToString(), "Failed to update car: Not found", "Failed");
            }
        }

        public async Task DeleteCarAsync(int id)
        {
            _logger.LogInformation("Deleting car {Id}", id);
            // 📨 Send Car Update Email (Delete)
            var car = await _carRepository.GetByIdAsync(id);
            if (car != null) {
                var emails = await _employeeManager.GetActiveEmployeeEmailsAsync();
                await _emailManager.SendCarUpdateEmail(emails, car, "Delete", "Status", "Active", "Deleted", "System/Admin");
            }

            await _carRepository.DeleteAsync(id);
            await _auditLogManager.LogAsync("Delete", "Car", id.ToString(), "Deleted car from fleet");
        }
        public async Task<List<CarListDto>> SearchCarsForListAsync(string? modelName = null, int? modelYear = null, int? categoryId = null, bool? isAvailable = null)
        {
            var cars = await _carRepository.SearchByFilterAsync(modelName, modelYear, categoryId, isAvailable);
            return _mapper.Map<List<CarListDto>>(cars);
        }

        public async Task<IEnumerable<CarListDto>> GetAllCarsForListAsync(string? name, int? categoryId, bool? available)
        {
            var query = _carRepository.Query().AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.ModelName.Contains(name) || c.PlateNumber.Contains(name));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }

            if (available.HasValue)
            {
                query = query.Where(c => c.IsAvailable == available.Value);
            }

            // Project to DTO directly to avoid fetching Blob columns (CarImage)
            return await query.Select(c => new CarListDto
            {
                CarId = c.CarId,
                PlateNumber = c.PlateNumber,
                ModelName = c.ModelName,
                ModelYear = c.ModelYear,
                Color = c.Color,
                PricePerDay = c.PricePerDay,
                IsAvailable = c.IsAvailable,
                CategoryId = c.CategoryId,
                CategoryName = c.Category != null ? c.Category.Name : null
            }).ToListAsync();
        }
    }

    public class CarProfile : Profile
    {
        public CarProfile()
        {
            CreateMap<Car, CarDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ReverseMap()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.CarImage, opt => opt.Condition(src => src.CarImage != null)); // only map image if provided
            
            CreateMap<Car, CarListDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));
        }
    }

}