using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data;

namespace RentACar.Web.Hubs;

[Authorize]
public class DriverTrackingHub : Hub
{
    private readonly IDriverRepository _driverRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly RentACarDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;

    public DriverTrackingHub(
        IDriverRepository driverRepository,
        IBookingRepository bookingRepository,
        RentACarDbContext dbContext,
        UserManager<IdentityUser> userManager)
    {
        _driverRepository = driverRepository;
        _bookingRepository = bookingRepository;
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public Task JoinBookingGroup(int bookingId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
    }

    public Task LeaveBookingGroup(int bookingId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
    }

    [Authorize(Roles = "Driver")]
    public async Task UpdateLocation(int bookingId, int driverId, double latitude, double longitude, DateTime timestamp,
        double? speed = null, double? heading = null, double? accuracy = null)
    {
        var userId = _userManager.GetUserId(Context.User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new HubException("Unauthorized");
        }

        var driver = await _driverRepository.GetByAspNetUserIdAsync(userId);
        if (driver == null || driver.DriverId != driverId)
        {
            throw new HubException("Invalid driver");
        }

        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        if (booking == null || booking.DriverId != driver.DriverId)
        {
            throw new HubException("Invalid booking");
        }

        if (booking.BookingStatus != null &&
            (booking.BookingStatus.Equals("returned", StringComparison.OrdinalIgnoreCase)
             || booking.BookingStatus.Equals("rejected", StringComparison.OrdinalIgnoreCase)
             || booking.BookingStatus.Equals("cancelled", StringComparison.OrdinalIgnoreCase)))
        {
            throw new HubException("Booking not active");
        }

        var ping = new DriverLocationPing
        {
            BookingId = booking.BookingId,
            DriverId = driver.DriverId,
            Latitude = (decimal)latitude,
            Longitude = (decimal)longitude,
            Speed = speed.HasValue ? (decimal?)speed.Value : null,
            Heading = heading.HasValue ? (decimal?)heading.Value : null,
            AccuracyMeters = accuracy.HasValue ? (decimal?)accuracy.Value : null,
            CreatedAt = timestamp == default ? DateTime.UtcNow : timestamp
        };

        _dbContext.DriverLocationPings.Add(ping);
        await _dbContext.SaveChangesAsync();

        await Clients.Group($"booking-{booking.BookingId}").SendAsync("DriverLocationUpdated", new
        {
            bookingId = booking.BookingId,
            driverId = driver.DriverId,
            latitude = ping.Latitude,
            longitude = ping.Longitude,
            speed = ping.Speed,
            heading = ping.Heading,
            accuracy = ping.AccuracyMeters,
            createdAt = ping.CreatedAt
        });
    }
}
