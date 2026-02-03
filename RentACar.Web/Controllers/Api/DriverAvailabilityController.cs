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

    public DriverAvailabilityController(
        UserManager<IdentityUser> userManager,
        IDriverRepository driverRepository,
        IDriverAvailabilityRepository availabilityRepository)
    {
        _userManager = userManager;
        _driverRepository = driverRepository;
        _availabilityRepository = availabilityRepository;
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

        var existing = (await _availabilityRepository.GetByDriverIdAndRangeAsync(driver.DriverId, request.Date, request.Date)).FirstOrDefault();

        if (existing != null)
        {
            existing.IsAvailable = request.IsAvailable;
            existing.StartTime = request.StartTime;
            existing.EndTime = request.EndTime;
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
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _availabilityRepository.AddAsync(record);
        }

        return Ok(new { success = true });
    }

    public class AvailabilityRequest
    {
        public DateOnly Date { get; set; }
        public bool IsAvailable { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
    }
}
