using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RentACar.Application.Managers;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data;
using RentACar.Web.Hubs;
using RentACar.Web.Models;

namespace RentACar.Web.Controllers.Api;

[Authorize(Roles = "Driver")]
[ApiController]
[Route("api/driver")]
public class DriverLocationController : ControllerBase
{
    private readonly IDriverRepository _driverRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly TripManager _tripManager;
    private readonly RentACarDbContext _dbContext;
    private readonly IHubContext<DriverTrackingHub> _hubContext;
    private readonly UserManager<IdentityUser> _userManager;

    public DriverLocationController(
        IDriverRepository driverRepository,
        IBookingRepository bookingRepository,
        TripManager tripManager,
        RentACarDbContext dbContext,
        IHubContext<DriverTrackingHub> hubContext,
        UserManager<IdentityUser> userManager)
    {
        _driverRepository = driverRepository;
        _bookingRepository = bookingRepository;
        _tripManager = tripManager;
        _dbContext = dbContext;
        _hubContext = hubContext;
        _userManager = userManager;
    }

    [HttpPost("location")]
    public async Task<IActionResult> PostLocation([FromBody] DriverLocationPingRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var driver = await _driverRepository.GetByAspNetUserIdAsync(userId);
        if (driver == null || !driver.IsActive || !driver.Employee.IsActive)
        {
            return Forbid();
        }

        var booking = await _bookingRepository.GetBookingByIdAsync(request.BookingId);
        if (booking == null || !booking.HasDriver || booking.DriverId != driver.DriverId)
        {
            return Forbid();
        }

        if (!IsActiveBooking(booking.BookingStatus))
        {
            return BadRequest("Booking is not active for tracking.");
        }

        var ping = new DriverLocationPing
        {
            BookingId = booking.BookingId,
            DriverId = driver.DriverId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Speed = request.Speed,
            Heading = request.Heading,
            AccuracyMeters = request.Accuracy,
            BatteryPercent = request.Battery,
            CreatedAt = DateTime.UtcNow
        };

        var tripResult = await _tripManager.UpdateDriverLocationAsync(
            booking.BookingId,
            driver.DriverId,
            request.Latitude,
            request.Longitude,
            DateTime.UtcNow);
        if (!tripResult.Success)
        {
            return BadRequest(tripResult.Message);
        }

        _dbContext.DriverLocationPings.Add(ping);
        await _dbContext.SaveChangesAsync();

        await _hubContext.Clients.Group($"booking-{booking.BookingId}").SendAsync("DriverLocationUpdated", new
        {
            bookingId = booking.BookingId,
            driverId = driver.DriverId,
            latitude = ping.Latitude,
            longitude = ping.Longitude,
            speed = ping.Speed,
            heading = ping.Heading,
            accuracy = ping.AccuracyMeters,
            battery = ping.BatteryPercent,
            createdAt = ping.CreatedAt
        });

        return Ok();
    }

    private static bool IsActiveBooking(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return true;
        }

        return !status.Equals("returned", StringComparison.OrdinalIgnoreCase)
               && !status.Equals("rejected", StringComparison.OrdinalIgnoreCase)
               && !status.Equals("cancelled", StringComparison.OrdinalIgnoreCase);
    }
}
