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
public class BookingsController : Controller
{
    private readonly RentACarDbContext _dbContext;
    private readonly DriverManager _driverManager;
    private readonly AuditLogManager _auditLogManager;

    public BookingsController(RentACarDbContext dbContext, DriverManager driverManager, AuditLogManager auditLogManager)
    {
        _dbContext = dbContext;
        _driverManager = driverManager;
        _auditLogManager = auditLogManager;
    }

    [HttpGet("/Driver/Bookings")]
    public async Task<IActionResult> Index(string? status, DateOnly? date)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var driver = userId != null ? await _driverManager.GetByAspNetUserIdAsync(userId) : null;
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

        var viewModel = bookings.Select(b => new DriverBookingListItemViewModel
        {
            BookingId = b.BookingId,
            CustomerName = b.Customer?.Name,
            CarModel = b.Car?.ModelName,
            StartDate = b.Startdate,
            EndDate = b.Enddate,
            Status = b.BookingStatus,
            PickupAddress = b.PickupAddress
        }).ToList();

        ViewBag.SelectedStatus = status;
        ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");

        return View("~/Areas/Driver/Views/Bookings/Index.cshtml", viewModel);
    }

    [HttpGet("/Driver/Bookings/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var driver = userId != null ? await _driverManager.GetByAspNetUserIdAsync(userId) : null;
        if (driver == null)
        {
            return Forbid();
        }

        var booking = await _dbContext.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Car)
            .FirstOrDefaultAsync(b => b.BookingId == id && b.DriverId == driver.DriverId);

        if (booking == null)
        {
            return NotFound();
        }

        var location = await _dbContext.DriverLocations
            .Where(l => l.BookingId == booking.BookingId && l.DriverId == driver.DriverId)
            .OrderByDescending(l => l.LastUpdatedUtc)
            .FirstOrDefaultAsync();

        var viewModel = new DriverBookingDetailsViewModel
        {
            BookingId = booking.BookingId,
            BookingStatus = booking.BookingStatus,
            StartDate = booking.Startdate,
            EndDate = booking.Enddate,
            CustomerName = booking.Customer?.Name,
            CustomerEmail = booking.Customer?.Email,
            CustomerPhone = booking.Customer?.PhoneNumber,
            CarModel = booking.Car?.ModelName,
            CarPlateNumber = booking.Car?.PlateNumber,
            PickupAddress = booking.PickupAddress,
            PickupLat = booking.PickupLat,
            PickupLng = booking.PickupLng,
            IsTrackingActive = location?.IsTrackingActive ?? false,
            LastTrackingUpdateUtc = location?.LastUpdatedUtc
        };

        return View("~/Areas/Driver/Views/Bookings/Details.cshtml", viewModel);
    }

    [HttpPost("/Driver/Bookings/{id:int}/StartTracking")]
    public async Task<IActionResult> StartTracking(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var driver = userId != null ? await _driverManager.GetByAspNetUserIdAsync(userId) : null;
        if (driver == null)
        {
            return Forbid();
        }

        var booking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.BookingId == id && b.DriverId == driver.DriverId);
        if (booking == null)
        {
            return NotFound();
        }

        var location = await _dbContext.DriverLocations
            .FirstOrDefaultAsync(l => l.BookingId == booking.BookingId && l.DriverId == driver.DriverId);

        if (location == null)
        {
            location = new Core.Entities.DriverLocation
            {
                BookingId = booking.BookingId,
                DriverId = driver.DriverId,
                Latitude = booking.PickupLat ?? 0,
                Longitude = booking.PickupLng ?? 0,
                LastUpdatedUtc = DateTime.UtcNow,
                IsTrackingActive = true
            };
            _dbContext.DriverLocations.Add(location);
        }
        else
        {
            location.IsTrackingActive = true;
            location.LastUpdatedUtc = DateTime.UtcNow;
            _dbContext.DriverLocations.Update(location);
        }

        await _dbContext.SaveChangesAsync();
        await _auditLogManager.LogEventAsync("DriverTracking.Started", "Booking", booking.BookingId.ToString(), $"Driver {driver.DriverId} started tracking.", null, "Success");

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("/Driver/Bookings/{id:int}/StopTracking")]
    public async Task<IActionResult> StopTracking(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var driver = userId != null ? await _driverManager.GetByAspNetUserIdAsync(userId) : null;
        if (driver == null)
        {
            return Forbid();
        }

        var location = await _dbContext.DriverLocations
            .FirstOrDefaultAsync(l => l.BookingId == id && l.DriverId == driver.DriverId);

        if (location == null)
        {
            return NotFound();
        }

        location.IsTrackingActive = false;
        location.LastUpdatedUtc = DateTime.UtcNow;
        _dbContext.DriverLocations.Update(location);
        await _dbContext.SaveChangesAsync();
        await _auditLogManager.LogEventAsync("DriverTracking.Stopped", "Booking", id.ToString(), $"Driver {driver.DriverId} stopped tracking.", null, "Success");

        return RedirectToAction(nameof(Details), new { id });
    }
}
