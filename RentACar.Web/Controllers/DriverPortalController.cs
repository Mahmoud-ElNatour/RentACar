using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
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

    public DriverPortalController(
        DriverManager driverManager,
        BookingManager bookingManager,
        CustomerManager customerManager,
        CarManager carManager,
        UserManager<IdentityUser> userManager)
    {
        _driverManager = driverManager;
        _bookingManager = bookingManager;
        _customerManager = customerManager;
        _carManager = carManager;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null)
        {
            return Forbid();
        }

        var bookings = await _bookingManager.GetBookingsByDriverIdAsync(driver.DriverId);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var model = new DriverDashboardViewModel
        {
            DriverId = driver.DriverId,
            DriverName = driver.FullName,
            DriverCode = driver.DriverCode,
            IsAvailable = driver.IsActive,
            TodayBookings = (await Task.WhenAll(bookings
                    .Where(b => b.Startdate <= today && b.Enddate >= today)
                    .Select(async b =>
                    {
                        var customer = await _customerManager.GetCustomerById(b.CustomerId);
                        return new DriverPortalBookingViewModel
                        {
                            BookingId = b.BookingId,
                            CustomerName = customer?.Name ?? "Customer",
                            PickupAddress = b.PickupAddress ?? "Pickup location",
                            StartDate = b.Startdate,
                            EndDate = b.Enddate,
                            BookingStatus = b.BookingStatus
                        };
                    })))
                .ToList()
        };

        ViewData["Title"] = "Driver Dashboard";
        ViewData["BodyClass"] = "bg-background-dark text-white";
        return View("~/Views/DriverPortal/Dashboard.cshtml", model);
    }

    [HttpGet]
    public async Task<IActionResult> Schedule()
    {
        var driver = await GetCurrentDriverAsync();
        if (driver == null)
        {
            return Forbid();
        }

        var availability = await _driverManager.GetDriverAvailabilityAsync(driver.DriverId);
        var bookings = await _bookingManager.GetBookingsByDriverIdAsync(driver.DriverId);

        var model = new DriverScheduleViewModel
        {
            DriverId = driver.DriverId,
            DriverName = driver.FullName,
            Availability = availability.Select(a => new DriverAvailabilityItemViewModel
            {
                DriverAvailabilityId = a.DriverAvailabilityId,
                StartDateTime = a.StartDateTime,
                EndDateTime = a.EndDateTime,
                IsAvailable = a.IsAvailable,
                IsRecurringWeekly = a.IsRecurringWeekly
            }).ToList(),
            UpcomingBookings = (await Task.WhenAll(bookings
                    .OrderBy(b => b.Startdate)
                    .Take(10)
                    .Select(async b =>
                    {
                        var customer = await _customerManager.GetCustomerById(b.CustomerId);
                        return new DriverPortalBookingViewModel
                        {
                            BookingId = b.BookingId,
                            CustomerName = customer?.Name ?? "Customer",
                            PickupAddress = b.PickupAddress ?? "Pickup location",
                            StartDate = b.Startdate,
                            EndDate = b.Enddate,
                            BookingStatus = b.BookingStatus
                        };
                    })))
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

        var model = new DriverBookingDetailsViewModel
        {
            BookingId = booking.BookingId,
            BookingStatus = booking.BookingStatus,
            CarName = car?.ModelName ?? "Vehicle",
            CarPlate = car?.PlateNumber ?? "N/A",
            CustomerName = customer?.Name ?? "Customer",
            PickupAddress = booking.PickupAddress ?? "Pickup location",
            PickupDateTime = booking.PickupDateTime,
            DriverCode = driver.DriverCode
        };

        ViewData["Title"] = "Booking Details";
        ViewData["BodyClass"] = "bg-background-dark text-white";
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
}
