using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RentACar.Application.Managers;
using RentACar.Infrastructure.Data;

namespace RentACar.Web.Hubs;

[Authorize]
public class DriverTrackingHub : Hub
{
    private readonly RentACarDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly AuditLogManager _auditLogManager;

    private static readonly TimeSpan MinUpdateInterval = TimeSpan.FromSeconds(4);

    public DriverTrackingHub(RentACarDbContext dbContext, IMemoryCache memoryCache, AuditLogManager auditLogManager)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
        _auditLogManager = auditLogManager;
    }

    public async Task JoinBookingTracking(int bookingId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new HubException("Unauthorized");
        }

        var booking = await _dbContext.Bookings
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking == null)
        {
            throw new HubException("Booking not found");
        }

        var isEmployee = Context.User?.IsInRole("Employee") == true || Context.User?.IsInRole("Admin") == true;
        var isOwner = booking.Customer?.aspNetUserId == userId;

        if (!isEmployee && !isOwner)
        {
            throw new HubException("Not authorized to track this booking");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
    }

    public async Task SendLocationUpdate(int bookingId, decimal lat, decimal lng)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new HubException("Unauthorized");
        }

        var booking = await _dbContext.Bookings
            .Include(b => b.Driver)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking?.Driver == null || booking.Driver.AspNetUserId != userId)
        {
            await _auditLogManager.LogEventAsync("DriverTracking.UnauthorizedUpdate", "Booking", bookingId.ToString(), "Unauthorized driver location update attempt.", null, "Failed");
            throw new HubException("Not authorized to update this booking");
        }

        var cacheKey = $"tracking-update-{bookingId}";
        if (_memoryCache.TryGetValue<DateTime>(cacheKey, out var lastUpdate) &&
            DateTime.UtcNow - lastUpdate < MinUpdateInterval)
        {
            return;
        }

        _memoryCache.Set(cacheKey, DateTime.UtcNow, MinUpdateInterval);

        var location = await _dbContext.DriverLocations
            .FirstOrDefaultAsync(l => l.BookingId == bookingId && l.DriverId == booking.Driver.DriverId);

        if (location == null)
        {
            location = new Core.Entities.DriverLocation
            {
                BookingId = bookingId,
                DriverId = booking.Driver.DriverId,
                Latitude = lat,
                Longitude = lng,
                LastUpdatedUtc = DateTime.UtcNow,
                IsTrackingActive = true
            };
            _dbContext.DriverLocations.Add(location);
        }
        else
        {
            location.Latitude = lat;
            location.Longitude = lng;
            location.LastUpdatedUtc = DateTime.UtcNow;
            location.IsTrackingActive = true;
            _dbContext.DriverLocations.Update(location);
        }

        await _dbContext.SaveChangesAsync();

        await Clients.Group($"booking-{bookingId}")
            .SendAsync("ReceiveLocationUpdate", new
            {
                bookingId,
                lat,
                lng,
                timestamp = location.LastUpdatedUtc
            });
    }
}
