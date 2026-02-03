using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.Managers;
using RentACar.Infrastructure.Data;
using RentACar.Web.Models;

namespace RentACar.Web.Controllers;

[Authorize(Roles = "Customer")]
public class TrackingController : Controller
{
    private readonly IConfiguration _config;

    private readonly BookingManager _bookingManager;
    private readonly DriverManager _driverManager;
    private readonly CustomerManager _customerManager;
    private readonly CarManager _carManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RentACarDbContext _dbContext;

    public TrackingController(
        BookingManager bookingManager,
        DriverManager driverManager,
        CustomerManager customerManager,
        CarManager carManager,
        UserManager<IdentityUser> userManager,
         IConfiguration config,
        RentACarDbContext dbContext)
    {
        _bookingManager = bookingManager;
        _driverManager = driverManager;
        _customerManager = customerManager;
        _carManager = carManager;
        _userManager = userManager;
        _dbContext = dbContext;
        _config = config;
    }

    [HttpGet("~/Tracking/CustomerLive/{id}")]
    public async Task<IActionResult> CustomerLive(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var customer = await _customerManager.GetCustomerByUsername(user.UserName ?? string.Empty);
        if (customer == null)
        {
            return Unauthorized();
        }

        var booking = await _bookingManager.GetBookingByIdAsync(id);
        if (booking == null || booking.CustomerId != customer.UserId)
        {
            return NotFound();
        }

        if (!booking.HasDriver || !booking.DriverId.HasValue || !IsActiveBooking(booking.BookingStatus))
        {
            return NotFound();
        }

        var driver = await _driverManager.GetDriverByIdAsync(booking.DriverId.Value);
        var car = await _carManager.GetCarByIdAsync(booking.CarId);

        var lastPing = await _dbContext.DriverLocationPings
            .Where(p => p.BookingId == booking.BookingId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        var model = new CustomerTrackingViewModel
        {
            BookingId = booking.BookingId,
            BookingStatus = booking.BookingStatus,
            CustomerName = customer.Name,
            DriverName = driver?.FullName ?? "Assigned Driver",
            DriverCode = driver?.DriverCode ?? "DR-0000",
            CarName = car?.ModelName ?? "Vehicle",
            CarPlate = car?.PlateNumber ?? "N/A",
            PickupAddress = booking.PickupAddress ?? "Pickup location",
            PickupDateTime = booking.PickupDateTime,
            LastLatitude = lastPing?.Latitude,
            LastLongitude = lastPing?.Longitude,
            PickupLatitude = booking.PickupLatitude,
            PickupLongitude = booking.PickupLongitude,
            LastPingAt = lastPing?.CreatedAt

        };

        ViewData["Title"] = "Live Driver Tracking";
        ViewData["BodyClass"] = "bg-background-dark text-white";
        ViewBag.GoogleMapsKey = _config["GOOGLE_MAPS_API_KEY"] ?? Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY");
        if (string.IsNullOrWhiteSpace(ViewBag.GoogleMapsKey))
            return StatusCode(500, "Google Maps API key is missing (GOOGLE_MAPS_API_KEY). Please ensure it is set in appsettings.json or as an environment variable.");


        return View("~/Views/Tracking/CustomerLive.cshtml", model);
    }

    private static bool IsActiveBooking(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return true;
        }

        return !status.Equals("returned", System.StringComparison.OrdinalIgnoreCase)
               && !status.Equals("rejected", System.StringComparison.OrdinalIgnoreCase)
               && !status.Equals("cancelled", System.StringComparison.OrdinalIgnoreCase);
    }
}
