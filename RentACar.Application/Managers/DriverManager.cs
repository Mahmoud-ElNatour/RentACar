using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Application.Managers;

public class DriverManager
{
    private readonly IDriverRepository _driverRepository;
    private readonly IDriverAvailabilityRepository _driverAvailabilityRepository;
    private readonly IMapper _mapper;
    private readonly AuditLogManager _auditLogManager;

    public DriverManager(
        IDriverRepository driverRepository,
        IDriverAvailabilityRepository driverAvailabilityRepository,
        IMapper mapper,
        AuditLogManager auditLogManager)
    {
        _driverRepository = driverRepository;
        _driverAvailabilityRepository = driverAvailabilityRepository;
        _mapper = mapper;
        _auditLogManager = auditLogManager;
    }

    public async Task<DriverDto?> GetDriverByIdAsync(int id)
    {
        var driver = await _driverRepository.GetByIdAsync(id);
        if (driver == null || !driver.IsActive || !driver.Employee.IsActive)
        {
            return null;
        }
        return _mapper.Map<DriverDto>(driver);
    }

    public async Task<DriverDto?> GetDriverByUserIdAsync(string userId)
    {
        var driver = await _driverRepository.GetByAspNetUserIdAsync(userId);
        if (driver == null || !driver.IsActive || !driver.Employee.IsActive)
        {
            return null;
        }
        return _mapper.Map<DriverDto>(driver);
    }

    public async Task<List<DriverDisplayDto>> GetAllDriversAsync()
    {
        var drivers = await _driverRepository.GetAllAsync();
        return drivers
            .Select(d => new DriverDisplayDto
            {
                DriverId = d.DriverId,
                DriverCode = d.DriverCode,
                FullName = d.Employee?.Name ?? d.FullName,
                Email = d.User?.Email ?? d.Email,
                Phone = d.User?.PhoneNumber ?? d.Phone,
                IsActive = d.IsActive
            })
            .ToList();
    }

    public async Task<List<DriverDisplayDto>> GetActiveDriversAsync()
    {
        var drivers = await _driverRepository.GetActiveAsync();
        return drivers
            .Select(d => new DriverDisplayDto
            {
                DriverId = d.DriverId,
                DriverCode = d.DriverCode,
                FullName = d.Employee?.Name ?? d.FullName,
                Email = d.User?.Email ?? d.Email,
                Phone = d.User?.PhoneNumber ?? d.Phone,
                IsActive = d.IsActive
            })
            .ToList();
    }

    public async Task<List<DriverAvailabilityDto>> GetDriverAvailabilityAsync(int driverId)
    {
        var availability = await _driverAvailabilityRepository.GetByDriverIdAsync(driverId);
        return _mapper.Map<List<DriverAvailabilityDto>>(availability);
    }

    public async Task AddAvailabilityAsync(int driverId, DriverAvailabilityDto dto)
    {
        var availability = new DriverAvailability
        {
            DriverId = driverId,
            StartDateTime = dto.StartDateTime,
            EndDateTime = dto.EndDateTime,
            IsRecurringWeekly = dto.IsRecurringWeekly,
            IsAvailable = dto.IsAvailable,
            CreatedAt = DateTime.UtcNow
        };

        await _driverAvailabilityRepository.AddAsync(availability);
        await _auditLogManager.LogAsync("Create", "DriverAvailability", availability.DriverAvailabilityId.ToString(),
            $"Driver {driverId} availability added.");
    }

}

public class DriverProfile : Profile
{
    public DriverProfile()
    {
        CreateMap<Driver, DriverDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Name : src.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : src.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User != null ? src.User.PhoneNumber : src.Phone))
            .ForMember(dest => dest.AllowedCategoryIds, opt => opt.MapFrom(src => src.AllowedCategories.Select(ac => ac.CategoryId)))
            .ReverseMap()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore())
            .ForMember(dest => dest.AllowedCategories, opt => opt.Ignore());

        CreateMap<DriverCreateDto, Driver>();
        CreateMap<DriverAvailability, DriverAvailabilityDto>().ReverseMap()
            .ForMember(dest => dest.Driver, opt => opt.Ignore());
    }
}
