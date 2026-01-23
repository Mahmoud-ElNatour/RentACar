using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Application.Managers;

public class DriverManager
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IDriverRepository _driverRepository;
    private readonly IDriverAvailabilityRepository _driverAvailabilityRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<DriverManager> _logger;
    private readonly AuditLogManager _auditLogManager;

    public DriverManager(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IDriverRepository driverRepository,
        IDriverAvailabilityRepository driverAvailabilityRepository,
        IBookingRepository bookingRepository,
        IMapper mapper,
        ILogger<DriverManager> logger,
        AuditLogManager auditLogManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _driverRepository = driverRepository;
        _driverAvailabilityRepository = driverAvailabilityRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _logger = logger;
        _auditLogManager = auditLogManager;
    }

    public async Task<DriverDto?> CreateDriverAsync(DriverCreateDto createDto)
    {
        _logger.LogInformation("Creating driver for {Email}", createDto.Email);
        var existingByUsername = await _userManager.FindByNameAsync(createDto.Email);
        if (existingByUsername != null)
        {
            throw new InvalidOperationException("Username is already in use by another user.");
        }

        var existingByEmail = await _userManager.FindByEmailAsync(createDto.Email);
        if (existingByEmail != null)
        {
            throw new InvalidOperationException("Email address is already registered.");
        }

        var user = new IdentityUser
        {
            UserName = createDto.Email,
            Email = createDto.Email,
            PhoneNumber = createDto.Phone,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, createDto.Password);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(errorMessage)
                ? "Unable to create driver account."
                : errorMessage);
        }

        if (!await _roleManager.RoleExistsAsync("Driver"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Driver"));
        }

        await _userManager.AddToRoleAsync(user, "Driver");

        var driver = _mapper.Map<Driver>(createDto);
        driver.DriverCode = GenerateDriverCode();
        driver.AspNetUserId = user.Id;
        driver.CreatedAt = DateTime.UtcNow;
        driver.IsActive = createDto.IsActive;
        await _driverRepository.AddAsync(driver);

        await _auditLogManager.LogAsync("Create", "Driver", driver.DriverId.ToString(),
            $"Created driver {driver.FullName} ({driver.Email})");

        return _mapper.Map<DriverDto>(driver);
    }

    public async Task<DriverDto?> GetDriverByIdAsync(int id)
    {
        var driver = await _driverRepository.GetByIdAsync(id);
        return _mapper.Map<DriverDto>(driver);
    }

    public async Task<DriverDto?> GetDriverByUserIdAsync(string userId)
    {
        var driver = await _driverRepository.GetByAspNetUserIdAsync(userId);
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
                FullName = d.FullName,
                Email = d.Email,
                Phone = d.Phone,
                IsActive = d.IsActive
            })
            .ToList();
    }

    public async Task UpdateDriverAsync(DriverDto driverDto)
    {
        var driver = await _driverRepository.GetByIdAsync(driverDto.DriverId);
        if (driver == null)
        {
            throw new KeyNotFoundException($"Driver with ID {driverDto.DriverId} not found.");
        }

        var user = await _userManager.FindByIdAsync(driver.AspNetUserId);
        if (user != null)
        {
            var existingByEmail = await _userManager.FindByEmailAsync(driverDto.Email);
            if (existingByEmail != null && existingByEmail.Id != user.Id)
            {
                throw new InvalidOperationException("Email address is already registered to another user.");
            }

            user.Email = driverDto.Email;
            user.UserName = driverDto.Email;
            user.PhoneNumber = driverDto.Phone;
            await _userManager.UpdateAsync(user);
        }

        driver.FullName = driverDto.FullName;
        driver.Phone = driverDto.Phone;
        driver.Email = driverDto.Email;
        driver.IsActive = driverDto.IsActive;
        driver.LicenseNumber = driverDto.LicenseNumber;
        driver.LicenseExpiry = driverDto.LicenseExpiry;
        driver.Languages = driverDto.Languages;
        driver.Notes = driverDto.Notes;
        driver.Rating = driverDto.Rating;
        driver.UpdatedAt = DateTime.UtcNow;

        await _driverRepository.UpdateAsync(driver);
        await _auditLogManager.LogAsync("Update", "Driver", driver.DriverId.ToString(),
            $"Updated driver profile: {driver.FullName}");
    }

    public async Task DeactivateDriverAsync(int id, bool isActive)
    {
        var driver = await _driverRepository.GetByIdAsync(id);
        if (driver == null)
        {
            throw new KeyNotFoundException($"Driver with ID {id} not found.");
        }

        driver.IsActive = isActive;
        driver.UpdatedAt = DateTime.UtcNow;
        await _driverRepository.UpdateAsync(driver);

        var user = await _userManager.FindByIdAsync(driver.AspNetUserId);
        if (user != null)
        {
            user.LockoutEnabled = !isActive;
            user.LockoutEnd = isActive ? null : DateTimeOffset.MaxValue;
            await _userManager.UpdateAsync(user);
        }

        await _auditLogManager.LogAsync("Update", "Driver", driver.DriverId.ToString(),
            $"{(isActive ? "Activated" : "Deactivated")} driver {driver.FullName}");
    }

    public async Task DeleteDriverAsync(int id)
    {
        var driver = await _driverRepository.GetByIdAsync(id);
        if (driver == null)
        {
            throw new KeyNotFoundException($"Driver with ID {id} not found.");
        }

        var hasBookings = (await _bookingRepository.GetBookingsByDriverIdAsync(id)).Any();
        if (hasBookings)
        {
            driver.IsActive = false;
            driver.UpdatedAt = DateTime.UtcNow;
            await _driverRepository.UpdateAsync(driver);

            var user = await _userManager.FindByIdAsync(driver.AspNetUserId);
            if (user != null)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
                await _userManager.UpdateAsync(user);
            }

            throw new InvalidOperationException("Driver has existing bookings and was marked inactive instead of being deleted.");
        }

        var deleteUser = await _userManager.FindByIdAsync(driver.AspNetUserId);
        if (deleteUser != null)
        {
            await _userManager.DeleteAsync(deleteUser);
        }

        await _driverRepository.DeleteAsync(id);
        await _auditLogManager.LogAsync("Delete", "Driver", id.ToString(), $"Deleted driver {id}");
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

    private static string GenerateDriverCode()
    {
        var random = Random.Shared.Next(1000, 9999);
        return $"DR-{random}";
    }
}

public class DriverProfile : Profile
{
    public DriverProfile()
    {
        CreateMap<Driver, DriverDto>().ReverseMap()
            .ForMember(dest => dest.User, opt => opt.Ignore());

        CreateMap<DriverCreateDto, Driver>();
        CreateMap<DriverAvailability, DriverAvailabilityDto>().ReverseMap()
            .ForMember(dest => dest.Driver, opt => opt.Ignore());
    }
}
