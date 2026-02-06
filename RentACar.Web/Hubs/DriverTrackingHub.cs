using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using RentACar.Application.Managers;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data;

namespace RentACar.Web.Hubs;

[Authorize]
public class DriverTrackingHub : Hub
{
    private readonly IDriverRepository _driverRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly TripManager _tripManager;
    private readonly RentACarDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;

    public DriverTrackingHub(
        IDriverRepository driverRepository,
        IBookingRepository bookingRepository,
        TripManager tripManager,
        RentACarDbContext dbContext,
        UserManager<IdentityUser> userManager)
    {
        _driverRepository = driverRepository;
        _bookingRepository = bookingRepository;
        _tripManager = tripManager;
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
    public async Task UpdateLocationForActiveTrips(double latitude, double longitude, DateTime timestamp,
        double? speed = null, double? heading = null, double? accuracy = null)
    {
        var userId = _userManager.GetUserId(Context.User);
        if (string.IsNullOrWhiteSpace(userId)) throw new HubException("Unauthorized");

        var driver = await _driverRepository.GetByAspNetUserIdAsync(userId);
        if (driver == null) throw new HubException("Driver not found");

        var activeBookingIds = await _tripManager.GetActiveBookingIdsForDriverAsync(driver.DriverId);

        if (!activeBookingIds.Any()) return;

        // Save ONE ping based on the first booking (or most relevant)
        // Ideally we associate with all, but DB schema usually links 1-to-1 or need pure logs table.
        // Assuming current Ping table has BookingId foreign key. We pick the first one to satisfy FK constraint.
        // Or if you want robust history, iterate and save for all. Choosing SAVE FOR ALL for completeness.

        foreach (var bookingId in activeBookingIds)
        {
            // Fanout Broadcast
            await Clients.Group($"booking-{bookingId}").SendAsync("DriverLocationUpdated", new
            {
                bookingId = bookingId,
                driverId = driver.DriverId,
                latitude,
                longitude,
                speed,
                heading,
                accuracy,
                createdAt = timestamp == default ? DateTime.UtcNow : timestamp
            });
        }

        // Save Ping Record (Limit to ONE record to avoid DB spam, linked to the first booking found)
        // If we want detailed history per trip, we'd save for all. 
        // Strategy: Save once linked to the first booking ID found.
        var primaryBookingId = activeBookingIds.First();

        var ping = new DriverLocationPing
        {
            BookingId = primaryBookingId,
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

        // Also update Trip table (LastLocation) for ALL active trips
        foreach (var bid in activeBookingIds)
        {
            await _tripManager.UpdateDriverLocationAsync(bid, driver.DriverId, (decimal)latitude, (decimal)longitude, timestamp);
        }
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

        var tripResult = await _tripManager.UpdateDriverLocationAsync(
            booking.BookingId,
            driver.DriverId,
            (decimal)latitude,
            (decimal)longitude,
            timestamp);
        if (!tripResult.Success)
        {
            throw new HubException(tripResult.Message);
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
