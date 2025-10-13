using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;

namespace RentACar.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AhlaBhalTalleController : Controller
{
    private readonly TravelBookingManager _travelBookingManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<AhlaBhalTalleController> _logger;

    public AhlaBhalTalleController(
        TravelBookingManager travelBookingManager,
        UserManager<IdentityUser> userManager,
        ILogger<AhlaBhalTalleController> logger)
    {
        _travelBookingManager = travelBookingManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet("~/AhlaBhalTalle")]
    [Authorize(Roles = "Customer,Admin,Employee")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index()
    {
        return View("~/Views/AhlaBhalTalle/Index.cshtml");
    }

    [HttpGet("~/ControlPanel/AhlaBhalTalle/Logs")]
    [Authorize(Roles = "Admin,Employee")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Logs()
    {
        return View("~/Views/ControlPanel/TravelActions.cshtml");
    }

    [HttpPost("hotel")]
    [Authorize(Roles = "Customer,Admin,Employee")]
    public async Task<IActionResult> BookHotel([FromBody] HotelBookingRequestDto request, CancellationToken cancellationToken)
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

        try
        {
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _travelBookingManager.BookHotelAsync(request, user, roles, cancellationToken);
            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status502BadGateway, result);
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Hotel booking failed for user {User}", user.UserName);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while booking hotel for user {User}", user.UserName);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("flight")]
    [Authorize(Roles = "Customer,Admin,Employee")]
    public async Task<IActionResult> BookFlight([FromBody] FlightBookingRequestDto request, CancellationToken cancellationToken)
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

        try
        {
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _travelBookingManager.BookFlightAsync(request, user, roles, cancellationToken);
            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status502BadGateway, result);
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Flight booking failed for user {User}", user.UserName);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while booking flight for user {User}", user.UserName);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("logs")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> GetLogs([FromQuery] string? customerUsername, [FromQuery] int limit = 100, [FromQuery] DateTime? fromUtc = null, [FromQuery] DateTime? toUtc = null)
    {
        limit = Math.Clamp(limit, 1, 500);

        var logs = await _travelBookingManager.GetLogsAsync(customerUsername, limit, fromUtc, toUtc);
        return Ok(logs);
    }
}
