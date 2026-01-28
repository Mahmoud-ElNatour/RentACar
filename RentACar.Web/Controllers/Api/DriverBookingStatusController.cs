using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.Managers;
using RentACar.Infrastructure.Data;
using RentACar.Web.Models;

namespace RentACar.Web.Controllers.Api;

[Authorize(Roles = "Driver")]
[ApiController]
[Route("api/driver/bookings")]
public class DriverBookingStatusController : ControllerBase
{
    private readonly BookingManager _bookingManager;
    private readonly DriverManager _driverManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RentACarDbContext _dbContext;

    public DriverBookingStatusController(
        BookingManager bookingManager,
        DriverManager driverManager,
        UserManager<IdentityUser> userManager,
        RentACarDbContext dbContext)
    {
        _bookingManager = bookingManager;
        _driverManager = driverManager;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpPost("{bookingId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int bookingId, [FromBody] DriverBookingStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var driver = await _driverManager.GetDriverByUserIdAsync(user.Id);
        if (driver == null)
        {
            return Forbid();
        }

        var booking = await _dbContext.Bookings.AsNoTracking()
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        if (booking == null || booking.DriverId != driver.DriverId)
        {
            return Forbid();
        }

        var updated = await _bookingManager.UpdateBookingStatusAsync(bookingId, request.Status);
        if (!updated)
        {
            return NotFound();
        }

        return Ok(new { status = request.Status });
    }
}
