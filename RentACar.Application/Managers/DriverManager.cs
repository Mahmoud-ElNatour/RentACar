using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Application.Managers;

public class DriverManager
{
    private readonly IDriverRepository _driverRepository;
    private readonly IDriverAvailabilityRepository _driverAvailabilityRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly UserManager<IdentityUser> _userManager;

    public DriverManager(
        IDriverRepository driverRepository,
        IDriverAvailabilityRepository driverAvailabilityRepository,
        IBookingRepository bookingRepository,
        UserManager<IdentityUser> userManager)
    {
        _driverRepository = driverRepository;
        _driverAvailabilityRepository = driverAvailabilityRepository;
        _bookingRepository = bookingRepository;
        _userManager = userManager;
    }

    public Task<Driver?> GetByAspNetUserIdAsync(string aspNetUserId)
    {
        return _driverRepository.GetByAspNetUserIdAsync(aspNetUserId);
    }

    public async Task<List<Driver>> GetAllDriversAsync()
    {
        var drivers = await _driverRepository.GetAllAsync();
        return drivers.ToList();
    }

    public async Task<List<Driver>> GetAvailableDriversAsync(DateOnly startDate, DateOnly endDate)
    {
        var drivers = await _driverRepository.GetActiveDriversAsync();
        var available = new List<Driver>();

        foreach (var driver in drivers.Where(d => d.IsAvailableManual))
        {
            if (await IsDriverAvailableAsync(driver.DriverId, startDate, endDate))
            {
                available.Add(driver);
            }
        }

        return available;
    }

    public async Task<bool> IsDriverAvailableAsync(int driverId, DateOnly startDate, DateOnly endDate)
    {
        var bookings = await _bookingRepository.GetBookingsByDriverIdAsync(driverId);
        if (bookings.Any(b => b.HasDriver && IsBlockingStatus(b.BookingStatus) && DatesOverlap(b.Startdate, b.Enddate, startDate, endDate)))
        {
            return false;
        }

        var availability = await _driverAvailabilityRepository.GetByDriverIdAsync(driverId);
        if (availability.Count == 0)
        {
            return true;
        }

        var start = startDate.ToDateTime(TimeOnly.MinValue);
        var end = endDate.ToDateTime(TimeOnly.MaxValue);
        return availability.Any(a => a.StartTime <= start && a.EndTime >= end);
    }

    public async Task<Driver> CreateDriverAsync(string aspNetUserId, string displayName, string? phoneNumber)
    {
        var user = await _userManager.FindByIdAsync(aspNetUserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var driver = new Driver
        {
            AspNetUserId = aspNetUserId,
            DisplayName = displayName,
            PhoneNumber = phoneNumber,
            IsActive = true,
            IsAvailableManual = true
        };

        return await _driverRepository.AddAsync(driver);
    }

    public async Task UpdateDriverAsync(Driver driver)
    {
        await _driverRepository.UpdateAsync(driver);
    }

    public async Task DeactivateDriverAsync(Driver driver)
    {
        driver.IsActive = false;
        driver.IsAvailableManual = false;
        await _driverRepository.UpdateAsync(driver);
    }

    private static bool DatesOverlap(DateOnly existingStart, DateOnly existingEnd, DateOnly newStart, DateOnly newEnd)
    {
        return existingStart <= newEnd && existingEnd >= newStart;
    }

    private static bool IsBlockingStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return true;
        }

        return !status.Equals("returned", StringComparison.OrdinalIgnoreCase)
               && !status.Equals("rejected", StringComparison.OrdinalIgnoreCase);
    }
}
