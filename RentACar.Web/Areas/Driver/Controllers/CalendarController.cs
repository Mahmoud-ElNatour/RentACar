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
public class CalendarController : Controller
{
    private readonly RentACarDbContext _dbContext;
    private readonly DriverManager _driverManager;

    public CalendarController(RentACarDbContext dbContext, DriverManager driverManager)
    {
        _dbContext = dbContext;
        _driverManager = driverManager;
    }

    [HttpGet("/Driver/Calendar")]
    public async Task<IActionResult> Index(int? year, int? month)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var driver = userId != null ? await _driverManager.GetByAspNetUserIdAsync(userId) : null;
        if (driver == null)
        {
            return Forbid();
        }

        var targetMonth = new DateTime(year ?? DateTime.Today.Year, month ?? DateTime.Today.Month, 1);
        var monthStart = DateOnly.FromDateTime(targetMonth);
        var monthEnd = DateOnly.FromDateTime(targetMonth.AddMonths(1).AddDays(-1));

        var bookings = await _dbContext.Bookings
            .Include(b => b.Customer)
            .Where(b => b.DriverId == driver.DriverId && b.Startdate <= monthEnd && b.Enddate >= monthStart)
            .OrderBy(b => b.Startdate)
            .ToListAsync();

        var availability = await _dbContext.DriverAvailabilities
            .Where(a => a.DriverId == driver.DriverId)
            .OrderBy(a => a.StartTime)
            .ToListAsync();

        var viewModel = new DriverCalendarViewModel
        {
            Month = targetMonth,
            Bookings = bookings.Select(b => new DriverBookingListItemViewModel
            {
                BookingId = b.BookingId,
                CustomerName = b.Customer?.Name,
                StartDate = b.Startdate,
                EndDate = b.Enddate,
                Status = b.BookingStatus,
                PickupAddress = b.PickupAddress
            }).ToList(),
            AvailabilityBlocks = availability.Select(a => new DriverAvailabilityBlockViewModel
            {
                DriverAvailabilityId = a.DriverAvailabilityId,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                IsRecurringWeekly = a.IsRecurringWeekly
            }).ToList()
        };

        return View("~/Areas/Driver/Views/Calendar/Index.cshtml", viewModel);
    }
}
