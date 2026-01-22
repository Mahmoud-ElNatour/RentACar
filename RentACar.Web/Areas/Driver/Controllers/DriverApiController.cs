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
[ApiController]
[Route("api/driver")]
public class DriverApiController : ControllerBase
{
    private readonly RentACarDbContext _dbContext;
    private readonly DriverManager _driverManager;

    public DriverApiController(RentACarDbContext dbContext, DriverManager driverManager)
    {
        _dbContext = dbContext;
        _driverManager = driverManager;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
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
            .Select(b => new DriverBookingApiItem
            {
                BookingId = b.BookingId,
                CustomerName = b.Customer?.Name,
                CarModel = b.Car?.ModelName,
                StartDate = b.Startdate.ToString("yyyy-MM-dd"),
                EndDate = b.Enddate.ToString("yyyy-MM-dd"),
                Status = b.BookingStatus,
                PickupAddress = b.PickupAddress
            })
            .ToList();

        var upcomingBookings = bookings
            .Where(b => b.Startdate > today)
            .Select(b => new DriverBookingApiItem
            {
                BookingId = b.BookingId,
                CustomerName = b.Customer?.Name,
                CarModel = b.Car?.ModelName,
                StartDate = b.Startdate.ToString("yyyy-MM-dd"),
                EndDate = b.Enddate.ToString("yyyy-MM-dd"),
                Status = b.BookingStatus,
                PickupAddress = b.PickupAddress
            })
            .ToList();

        var response = new DriverDashboardApiResponse
        {
            DriverName = driver.DisplayName,
            IsAvailableManual = driver.IsAvailableManual,
            IsOnTrip = bookings.Any(b => b.Startdate <= today && b.Enddate >= today
                                         && !string.Equals(b.BookingStatus, "Returned", StringComparison.OrdinalIgnoreCase)),
            TodayCount = todayBookings.Count,
            UpcomingCount = upcomingBookings.Count,
            TodayBookings = todayBookings,
            UpcomingBookings = upcomingBookings
        };

        return Ok(response);
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> Bookings([FromQuery] string? status, [FromQuery] DateOnly? date)
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

        var bookingsQuery = _dbContext.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Car)
            .Where(b => b.DriverId == driver.DriverId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            bookingsQuery = bookingsQuery.Where(b => b.BookingStatus == status);
        }

        if (date.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.Startdate <= date && b.Enddate >= date);
        }

        var bookings = await bookingsQuery.OrderByDescending(b => b.Startdate).ToListAsync();

        var response = bookings.Select(b => new DriverBookingApiItem
        {
            BookingId = b.BookingId,
            CustomerName = b.Customer?.Name,
            CarModel = b.Car?.ModelName,
            StartDate = b.Startdate.ToString("yyyy-MM-dd"),
            EndDate = b.Enddate.ToString("yyyy-MM-dd"),
            Status = b.BookingStatus,
            PickupAddress = b.PickupAddress
        }).ToList();

        return Ok(response);
    }
}
