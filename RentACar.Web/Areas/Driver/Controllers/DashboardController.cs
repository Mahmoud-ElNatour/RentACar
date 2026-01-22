using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.Managers;
using RentACar.Infrastructure.Data;
using RentACar.Web.Models.Driver;

namespace RentACar.Web.Areas.Driver.Controllers;

[Area("Driver")]
[Authorize(Roles = "Driver")]
public class DashboardController : Controller
{
    private readonly RentACarDbContext _dbContext;
    private readonly DriverManager _driverManager;

    public DashboardController(RentACarDbContext dbContext, DriverManager driverManager)
    {
        _dbContext = dbContext;
        _driverManager = driverManager;
    }

    [HttpGet("/Driver/Dashboard")]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var driver = await _driverManager.GetByAspNetUserIdAsync(userId);
        if (driver == null)
        {
            return Forbid();
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var bookings = await _dbContext.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Car)
            .Where(b => b.DriverId == driver.DriverId)
            .OrderBy(b => b.Startdate)
            .ToListAsync();

        var todayBookings = bookings
            .Where(b => b.Startdate <= today && b.Enddate >= today)
            .Select(b => new DriverBookingListItemViewModel
            {
                BookingId = b.BookingId,
                CustomerName = b.Customer?.Name,
                CarModel = b.Car?.ModelName,
                StartDate = b.Startdate,
                EndDate = b.Enddate,
                Status = b.BookingStatus,
                PickupAddress = b.PickupAddress
            })
            .ToList();

        var upcoming = bookings
            .Where(b => b.Startdate > today)
            .Take(5)
            .Select(b => new DriverBookingListItemViewModel
            {
                BookingId = b.BookingId,
                CustomerName = b.Customer?.Name,
                CarModel = b.Car?.ModelName,
                StartDate = b.Startdate,
                EndDate = b.Enddate,
                Status = b.BookingStatus,
                PickupAddress = b.PickupAddress
            })
            .ToList();

        var isOnTrip = bookings.Any(b => b.Startdate <= today && b.Enddate >= today && !string.Equals(b.BookingStatus, "Returned", StringComparison.OrdinalIgnoreCase));

        var viewModel = new DriverDashboardViewModel
        {
            DriverName = driver.DisplayName,
            IsAvailableManual = driver.IsAvailableManual,
            IsOnTrip = isOnTrip,
            TodayBookings = todayBookings,
            UpcomingBookings = upcoming
        };

        return View("~/Areas/Driver/Views/Dashboard/Index.cshtml", viewModel);
    }

    [HttpPost("/Driver/Dashboard/ToggleAvailability")]
    public async Task<IActionResult> ToggleAvailability()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var driver = await _driverManager.GetByAspNetUserIdAsync(userId);
        if (driver == null)
        {
            return Forbid();
        }

        driver.IsAvailableManual = !driver.IsAvailableManual;
        await _driverManager.UpdateDriverAsync(driver);

        return RedirectToAction(nameof(Index));
    }
}
