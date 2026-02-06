using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers.Api;

[Authorize(Roles = "Driver")]
[Route("api/driver/availability")]
[ApiController]
public class DriverAvailabilityController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IDriverRepository _driverRepository;
    private readonly IDriverAvailabilityRepository _availabilityRepository;
    private readonly ILogger<DriverAvailabilityController> _logger;
    public DriverAvailabilityController(
        UserManager<IdentityUser> userManager,
        IDriverRepository driverRepository,
        IDriverAvailabilityRepository availabilityRepository,
        ILogger<DriverAvailabilityController> logger)
    {
        _userManager = userManager;
        _driverRepository = driverRepository;
        _availabilityRepository = availabilityRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailability(DateOnly from, DateOnly to)
    {
        var userId = _userManager.GetUserId(User);
        var driver = await _driverRepository.GetByAspNetUserIdAsync(userId!);
        if (driver == null) return Forbid();

        var records = await _availabilityRepository.GetByDriverIdAndRangeAsync(driver.DriverId, from, to);
        return Ok(records.Select(a => new
        {
            a.DriverAvailabilityId,
            a.Date,
            a.IsAvailable,
            a.StartTime,
            a.EndTime
        }));
    }

    [HttpPost]
    public async Task<IActionResult> UpsertAvailability([FromBody] AvailabilityRequest request)
    {
        var userId = _userManager.GetUserId(User);
        var driver = await _driverRepository.GetByAspNetUserIdAsync(userId!);
        if (driver == null) return Forbid();

        // FULL-DAY enforcement (Option 1)
        TimeOnly? startTime = null;
        TimeOnly? endTime = null;

        if (request.IsAvailable)
        {
            startTime = new TimeOnly(0, 0);
            endTime = new TimeOnly(23, 59);
        }

        var existing = (await _availabilityRepository
            .GetByDriverIdAndRangeAsync(driver.DriverId, request.Date, request.Date))
            .FirstOrDefault();

        if (existing != null)
        {
            existing.IsAvailable = request.IsAvailable;
            existing.StartTime = startTime;
            existing.EndTime = endTime;
            existing.UpdatedAt = DateTime.UtcNow;

            await _availabilityRepository.UpdateAsync(existing);
        }
        else
        {
            var record = new DriverAvailability
            {
                DriverId = driver.DriverId,
                Date = request.Date,
                IsAvailable = request.IsAvailable,
                StartTime = startTime,
                EndTime = endTime,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _availabilityRepository.AddAsync(record);
        }

        return Ok(new { success = true });
    }

    [HttpPost("range")]
    public async Task<IActionResult> UpsertAvailabilityRange([FromBody] AvailabilityRangeRequest request)
    {
        var userId = _userManager.GetUserId(User);
        _logger.LogInformation("UpsertAvailabilityRange Request: User {UserId}, Range {From} to {To}, Available: {IsAvailable}", userId, request.From, request.To, request.IsAvailable);

        var driver = await _driverRepository.GetByAspNetUserIdAsync(userId!);
        if (driver == null)
        {
            _logger.LogWarning("UpsertAvailabilityRange: Driver not found for user {UserId}", userId);
            return Forbid();
        }

        _logger.LogInformation("UpsertAvailabilityRange: Found Driver {DriverId}. Saving range...", driver.DriverId);

        await _availabilityRepository.UpsertRangeAsync(driver.DriverId, request.From, request.To, request.IsAvailable);

        _logger.LogInformation("UpsertAvailabilityRange: Successfully saved range for Driver {DriverId}.", driver.DriverId);

        return Ok(new { success = true });
    }

    public class AvailabilityRequest
    {
        public DateOnly Date { get; set; }
        public bool IsAvailable { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
    }

    public class AvailabilityRangeRequest
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public bool IsAvailable { get; set; }
    }
}
