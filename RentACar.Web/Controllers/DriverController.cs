using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;

namespace RentACar.Web.Controllers;

[Authorize(Roles = "Admin,Employee,Driver")]
[ApiController]
[Route("api/[controller]")]
public class DriverController : Controller
{
    private readonly DriverManager _driverManager;
    private readonly ILogger<DriverController> _logger;

    public DriverController(DriverManager driverManager, ILogger<DriverController> logger)
    {
        _driverManager = driverManager;
        _logger = logger;
    }

    [HttpGet("~/Driver")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index()
    {
        return View("~/Views/ControlPanel/Driver/Index.cshtml");
    }

    [HttpGet("~/Driver/Add")]
    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult AddForm()
    {
        return PartialView("~/Views/ControlPanel/Driver/_DriverFormPartial.cshtml", new DriverDto { IsActive = true });
    }

    [HttpGet("~/Driver/Edit/{id}")]
    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> EditForm(int id)
    {
        var driver = await _driverManager.GetDriverByIdAsync(id);
        if (driver == null) return NotFound();
        return PartialView("~/Views/ControlPanel/Driver/_DriverFormPartial.cshtml", driver);
    }

    [HttpGet("~/Driver/Delete/{id}")]
    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> DeleteForm(int id)
    {
        var driver = await _driverManager.GetDriverByIdAsync(id);
        if (driver == null) return NotFound();
        return PartialView("~/Views/ControlPanel/Driver/_DeleteDriverPartial.cshtml", driver);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DriverDisplayDto>>> Get([FromQuery] string? search, [FromQuery] bool? active)
    {
        var drivers = await _driverManager.GetAllDriversAsync();
        if (!string.IsNullOrWhiteSpace(search))
        {
            drivers = drivers.Where(d =>
                d.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                d.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                d.DriverCode.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (active.HasValue)
        {
            drivers = drivers.Where(d => d.IsActive == active.Value).ToList();
        }

        return Ok(drivers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DriverDto>> Get(int id)
    {
        var driver = await _driverManager.GetDriverByIdAsync(id);
        if (driver == null) return NotFound();
        return Ok(driver);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DriverDto>> Create([FromBody] DriverCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _driverManager.CreateDriverAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created!.DriverId }, created);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create driver: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] DriverDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != dto.DriverId)
        {
            return BadRequest("Driver ID mismatch.");
        }

        try
        {
            await _driverManager.UpdateDriverAsync(dto);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update driver {Id}: {Message}", id, ex.Message);
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _driverManager.DeactivateDriverAsync(id, false);
        return NoContent();
    }

    [HttpPost("{id}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(int id)
    {
        await _driverManager.DeactivateDriverAsync(id, true);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _driverManager.DeleteDriverAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Driver {Id} could not be deleted: {Message}", id, ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database constraint prevented deleting driver {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Unable to delete driver because related records exist. Remove the related data before deleting the driver.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting driver {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An unexpected error occurred while deleting the driver. Please try again later.");
        }
    }
}
