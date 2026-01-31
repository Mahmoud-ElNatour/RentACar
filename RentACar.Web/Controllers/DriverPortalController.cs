using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Infrastructure.Data;
using RentACar.Web.Models;

namespace RentACar.Web.Controllers;

[Authorize(Roles = "Driver")]
public class DriverPortalController : Controller
{
    private readonly DriverManager _driverManager;
    private readonly BookingManager _bookingManager;
    private readonly CustomerManager _customerManager;
    private readonly CarManager _carManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RentACarDbContext _dbContext;
    private readonly IConfiguration _config;

    public DriverPortalController(
        DriverManager driverManager,
        BookingManager bookingManager,
        CustomerManager customerManager,
        CarManager carManager,
        UserManager<IdentityUser> userManager,
        RentACarDbContext dbContext,
        IConfiguration config)
    {
        _driverManager = driverManager;
        _bookingManager = bookingManager;
        _customerManager = customerManager;
        _carManager = carManager;
        _userManager = userManager;
        _dbContext = dbContext;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null)
        {
            return Forbid();
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var todayBookings = await (from booking in _dbContext.Bookings.AsNoTracking()
            join customer in _dbContext.Customers.AsNoTracking()
                on booking.CustomerId equals customer.UserId into customerGroup
            from customer in customerGroup.DefaultIfEmpty()
            where booking.DriverId == driver.DriverId
                  && booking.Startdate <= today
                  && booking.Enddate >= today
            orderby booking.Startdate
            select new DriverPortalBookingViewModel
            {
                BookingId = booking.BookingId,
                CustomerName = customer != null ? customer.Name : "Customer",
                PickupLocationLabel = booking.PickupLocationLabel ?? booking.PickupAddress ?? "Pickup pin",
                PickupLatitude = booking.PickupLatitude,
                PickupLongitude = booking.PickupLongitude,
                StartDate = booking.Startdate,
                EndDate = booking.Enddate,
                BookingStatus = booking.BookingStatus ?? "Scheduled"
            }).ToListAsync();

        var model = new DriverDashboardViewModel
        {
            DriverId = driver.DriverId,
            DriverName = driver.FullName,
            DriverCode = driver.DriverCode,
            IsAvailable = driver.IsActive,
            TodayBookings = todayBookings
        };

        ViewData["Title"] = "Driver Dashboard";
        ViewData["BodyClass"] = "bg-background-dark text-white";
        return View("~/Views/DriverPortal/Dashboard.cshtml", model);
    }

    [HttpGet]
    public async Task<IActionResult> Schedule()
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null) return Forbid();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var gridStart = monthStart.ToDateTime(TimeOnly.MinValue).AddDays(-(int)monthStart.DayOfWeek);
        var gridEnd = monthEnd.ToDateTime(TimeOnly.MinValue).AddDays(6 - (int)monthEnd.DayOfWeek);
        var gridStartDate = DateOnly.FromDateTime(gridStart);
        var gridEndDate = DateOnly.FromDateTime(gridEnd);

        var availability = await _driverManager.GetDriverAvailabilityAsync(driver.DriverId);
        var bookings = await _bookingManager.GetBookingsByDriverIdAsync(driver.DriverId);

        var availabilityItems = availability.Select(a => new DriverAvailabilityItemViewModel
        {
            DriverAvailabilityId = a.DriverAvailabilityId,
            StartDateTime = a.StartDateTime,
            EndDateTime = a.EndDateTime,
            IsAvailable = a.IsAvailable,
            IsRecurringWeekly = a.IsRecurringWeekly
        }).ToList();

        var bookingItems = new List<DriverScheduleBookingItemViewModel>();
        foreach (var booking in bookings.OrderBy(b => b.Startdate))
        {
            var customer = await _customerManager.GetCustomerById(booking.CustomerId);
            bookingItems.Add(new DriverScheduleBookingItemViewModel
            {
                BookingId = booking.BookingId,
                CustomerName = customer?.Name ?? "Customer",
                StartDate = booking.Startdate,
                EndDate = booking.Enddate,
                PickupDateTime = booking.PickupDateTime,
                PickupLocationLabel = booking.PickupLocationLabel ?? booking.PickupLocationName ?? booking.PickupAddress ?? "Pickup location",
                BookingStatus = booking.BookingStatus ?? "Pending"
            });
        }

        var days = new List<DriverScheduleDayViewModel>();
        for (var date = gridStartDate; date <= gridEndDate; date = date.AddDays(1))
        {
            var dayStart = date.ToDateTime(TimeOnly.MinValue);
            var dayEnd = date.ToDateTime(TimeOnly.MaxValue);
            var dayAvailability = availabilityItems
                .Where(a => a.IsAvailable && IsAvailabilityOnDate(a, date, dayStart, dayEnd))
                .ToList();
            var dayBookings = bookingItems
                .Where(b => b.StartDate <= date && b.EndDate >= date)
                .ToList();

            days.Add(new DriverScheduleDayViewModel
            {
                Date = date,
                IsToday = date == today,
                IsCurrentMonth = date >= monthStart && date <= monthEnd,
                HasAvailability = dayAvailability.Any(),
                HasBookings = dayBookings.Any(),
                AvailabilityBlocks = dayAvailability,
                Bookings = dayBookings
            });
        }

        var model = new DriverScheduleViewModel
        {
            DriverId = driver.DriverId,
            DriverName = driver.FullName,
            MonthStart = monthStart,
            MonthEnd = monthEnd,
            Days = days,
            Availability = availabilityItems,
            Bookings = bookingItems,
            UpcomingBookings = bookingItems
                .OrderBy(b => b.StartDate)
                .Take(10)
                .Select(b => new DriverPortalBookingViewModel
                {
                    BookingId = b.BookingId,
                    CustomerName = b.CustomerName,
                    PickupLocationLabel = b.PickupLocationLabel,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    BookingStatus = b.BookingStatus
                })
                .ToList()
        };

        ViewData["Title"] = "Driver Schedule";
        ViewData["BodyClass"] = "bg-background-dark text-white";
        return View("~/Views/DriverPortal/Schedule.cshtml", model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAvailability(DriverAvailabilityDto dto)
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null)
        {
            return Forbid();
        }

        await _driverManager.AddAvailabilityAsync(driver.DriverId, dto);
        return RedirectToAction(nameof(Schedule));
    }

    [HttpGet]
    public async Task<IActionResult> BookingDetails(int id)
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null)
        {
            return Forbid();
        }

        var booking = await _bookingManager.GetBookingByIdAsync(id);
        if (booking == null || booking.DriverId != driver.DriverId)
        {
            return NotFound();
        }

        var car = await _carManager.GetCarByIdAsync(booking.CarId);
        var customer = await _customerManager.GetCustomerById(booking.CustomerId);

        var lastPing = await _dbContext.DriverLocationPings.AsNoTracking()
            .Where(p => p.BookingId == booking.BookingId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        var model = new DriverBookingDetailsViewModel
        {
            BookingId = booking.BookingId,
            BookingStatus = booking.BookingStatus,
            CarName = car?.ModelName ?? "Vehicle",
            CarPlate = car?.PlateNumber ?? "N/A",
            CustomerName = customer?.Name ?? "Customer",
            PickupLocationLabel = booking.PickupLocationLabel ?? booking.PickupAddress ?? "Pickup pin",
            PickupLatitude = booking.PickupLatitude,
            PickupLongitude = booking.PickupLongitude,
            DriverLatitude = lastPing != null ? (double?)lastPing.Latitude : null,
            DriverLongitude = lastPing != null ? (double?)lastPing.Longitude : null,
            PickupDateTime = booking.PickupDateTime,
            DriverCode = driver.DriverCode,
            DriverId = driver.DriverId
        };

        ViewData["Title"] = "Booking Details";
        ViewData["BodyClass"] = "bg-background-dark text-white";
        ViewBag.GoogleMapsKey = _config["GOOGLE_MAPS_API_KEY"];
        return View("~/Views/DriverPortal/BookingDetails.cshtml", model);
    }

    private async Task<DriverDto?> GetCurrentDriverAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return null;
        }

        return await _driverManager.GetDriverByUserIdAsync(user.Id);
    }

    private static bool IsAvailabilityOnDate(DriverAvailabilityItemViewModel availability, DateOnly date, DateTime dayStart, DateTime dayEnd)
    {
        if (availability.IsRecurringWeekly)
        {
            return availability.StartDateTime.DayOfWeek == dayStart.DayOfWeek;
        }

        return availability.StartDateTime <= dayEnd && availability.EndDateTime >= dayStart;
    }
}
