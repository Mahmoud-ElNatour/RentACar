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
public class AvailabilityController : Controller
{
    private readonly RentACarDbContext _dbContext;
    private readonly DriverManager _driverManager;

    public AvailabilityController(RentACarDbContext dbContext, DriverManager driverManager)
    {
        _dbContext = dbContext;
        _driverManager = driverManager;
    }

    [HttpGet("/Driver/Availability")]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var driver = userId != null ? await _driverManager.GetByAspNetUserIdAsync(userId) : null;
        if (driver == null)
        {
            return Forbid();
        }

        var blocks = await _dbContext.DriverAvailabilities
            .Where(a => a.DriverId == driver.DriverId)
            .OrderBy(a => a.StartTime)
            .ToListAsync();

        var viewModel = blocks.Select(a => new DriverAvailabilityBlockViewModel
        {
            DriverAvailabilityId = a.DriverAvailabilityId,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            IsRecurringWeekly = a.IsRecurringWeekly
        }).ToList();

        return View("~/Areas/Driver/Views/Availability/Index.cshtml", viewModel);
    }

    [HttpPost("/Driver/Availability/Create")]
    public async Task<IActionResult> Create(DriverAvailabilityBlockViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["AvailabilityError"] = "Please provide valid availability times.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var driver = userId != null ? await _driverManager.GetByAspNetUserIdAsync(userId) : null;
        if (driver == null)
        {
            return Forbid();
        }

        if (model.EndTime <= model.StartTime)
        {
            TempData["AvailabilityError"] = "End time must be after start time.";
            return RedirectToAction(nameof(Index));
        }

        var overlaps = await _dbContext.DriverAvailabilities
            .Where(a => a.DriverId == driver.DriverId && a.StartTime < model.EndTime && a.EndTime > model.StartTime)
            .AnyAsync();

        if (overlaps)
        {
            TempData["AvailabilityError"] = "Availability block overlaps an existing block.";
            return RedirectToAction(nameof(Index));
        }

        var bookingConflict = await _dbContext.Bookings
            .Where(b => b.DriverId == driver.DriverId && b.Startdate.ToDateTime(TimeOnly.MinValue) < model.EndTime
                        && b.Enddate.ToDateTime(TimeOnly.MaxValue) > model.StartTime)
            .AnyAsync();

        if (bookingConflict)
        {
            TempData["AvailabilityError"] = "Availability conflicts with an existing booking.";
            return RedirectToAction(nameof(Index));
        }

        var entity = new Core.Entities.DriverAvailability
        {
            DriverId = driver.DriverId,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            IsRecurringWeekly = model.IsRecurringWeekly
        };

        _dbContext.DriverAvailabilities.Add(entity);
        await _dbContext.SaveChangesAsync();

        TempData["AvailabilitySuccess"] = "Availability saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/Driver/Availability/Edit")]
    public async Task<IActionResult> Edit(DriverAvailabilityBlockViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["AvailabilityError"] = "Please provide valid availability times.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var driver = userId != null ? await _driverManager.GetByAspNetUserIdAsync(userId) : null;
        if (driver == null)
        {
            return Forbid();
        }

        var existing = await _dbContext.DriverAvailabilities
            .FirstOrDefaultAsync(a => a.DriverAvailabilityId == model.DriverAvailabilityId && a.DriverId == driver.DriverId);

        if (existing == null)
        {
            return NotFound();
        }

        var overlaps = await _dbContext.DriverAvailabilities
            .Where(a => a.DriverId == driver.DriverId && a.DriverAvailabilityId != model.DriverAvailabilityId
                        && a.StartTime < model.EndTime && a.EndTime > model.StartTime)
            .AnyAsync();

        if (overlaps)
        {
            TempData["AvailabilityError"] = "Availability block overlaps an existing block.";
            return RedirectToAction(nameof(Index));
        }

        var bookingConflict = await _dbContext.Bookings
            .Where(b => b.DriverId == driver.DriverId && b.Startdate.ToDateTime(TimeOnly.MinValue) < model.EndTime
                        && b.Enddate.ToDateTime(TimeOnly.MaxValue) > model.StartTime)
            .AnyAsync();

        if (bookingConflict)
        {
            TempData["AvailabilityError"] = "Availability conflicts with an existing booking.";
            return RedirectToAction(nameof(Index));
        }

        existing.StartTime = model.StartTime;
        existing.EndTime = model.EndTime;
        existing.IsRecurringWeekly = model.IsRecurringWeekly;

        _dbContext.DriverAvailabilities.Update(existing);
        await _dbContext.SaveChangesAsync();

        TempData["AvailabilitySuccess"] = "Availability updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/Driver/Availability/Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var driver = userId != null ? await _driverManager.GetByAspNetUserIdAsync(userId) : null;
        if (driver == null)
        {
            return Forbid();
        }

        var existing = await _dbContext.DriverAvailabilities
            .FirstOrDefaultAsync(a => a.DriverAvailabilityId == id && a.DriverId == driver.DriverId);

        if (existing == null)
        {
            return NotFound();
        }

        _dbContext.DriverAvailabilities.Remove(existing);
        await _dbContext.SaveChangesAsync();
        TempData["AvailabilitySuccess"] = "Availability deleted.";
        return RedirectToAction(nameof(Index));
    }
}
