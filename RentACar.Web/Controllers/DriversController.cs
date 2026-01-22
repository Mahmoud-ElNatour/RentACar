using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.Managers;
using RentACar.Web.Models.Driver;

namespace RentACar.Web.Controllers;

[Authorize(Roles = "Admin,Employee")]
public class DriversController : Controller
{
    private readonly DriverManager _driverManager;
    private readonly AuditLogManager _auditLogManager;

    public DriversController(DriverManager driverManager, AuditLogManager auditLogManager)
    {
        _driverManager = driverManager;
        _auditLogManager = auditLogManager;
    }

    [HttpGet("~/ControlPanel/Drivers")]
    public async Task<IActionResult> Index()
    {
        var drivers = await _driverManager.GetAllDriversAsync();
        return View("~/Views/ControlPanel/Drivers/Index.cshtml", drivers);
    }

    [HttpGet("~/ControlPanel/Drivers/Create")]
    public IActionResult Create()
    {
        return View("~/Views/ControlPanel/Drivers/Create.cshtml", new DriverFormViewModel());
    }

    [HttpPost("~/ControlPanel/Drivers/Create")]
    public async Task<IActionResult> Create(DriverFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/ControlPanel/Drivers/Create.cshtml", model);
        }

        var created = await _driverManager.CreateDriverAsync(model.AspNetUserId, model.DisplayName, model.PhoneNumber);
        await _auditLogManager.LogEventAsync("Driver.Created", "Driver", created.DriverId.ToString(), $"Driver created for user {created.AspNetUserId}.", null, "Success");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("~/ControlPanel/Drivers/Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var drivers = await _driverManager.GetAllDriversAsync();
        var driver = drivers.FirstOrDefault(d => d.DriverId == id);
        if (driver == null)
        {
            return NotFound();
        }

        var model = new DriverFormViewModel
        {
            DriverId = driver.DriverId,
            AspNetUserId = driver.AspNetUserId,
            DisplayName = driver.DisplayName,
            PhoneNumber = driver.PhoneNumber,
            IsActive = driver.IsActive,
            IsAvailableManual = driver.IsAvailableManual
        };

        return View("~/Views/ControlPanel/Drivers/Edit.cshtml", model);
    }

    [HttpPost("~/ControlPanel/Drivers/Edit")]
    public async Task<IActionResult> Edit(DriverFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/ControlPanel/Drivers/Edit.cshtml", model);
        }

        var drivers = await _driverManager.GetAllDriversAsync();
        var driver = drivers.FirstOrDefault(d => d.DriverId == model.DriverId);
        if (driver == null)
        {
            return NotFound();
        }

        driver.DisplayName = model.DisplayName;
        driver.PhoneNumber = model.PhoneNumber;
        driver.IsActive = model.IsActive;
        driver.IsAvailableManual = model.IsAvailableManual;

        await _driverManager.UpdateDriverAsync(driver);
        await _auditLogManager.LogEventAsync("Driver.Updated", "Driver", driver.DriverId.ToString(), $"Driver {driver.DriverId} updated.", null, "Success");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("~/ControlPanel/Drivers/Deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var drivers = await _driverManager.GetAllDriversAsync();
        var driver = drivers.FirstOrDefault(d => d.DriverId == id);
        if (driver == null)
        {
            return NotFound();
        }

        await _driverManager.DeactivateDriverAsync(driver);
        await _auditLogManager.LogEventAsync("Driver.Deactivated", "Driver", driver.DriverId.ToString(), $"Driver {driver.DriverId} deactivated.", null, "Success");
        return RedirectToAction(nameof(Index));
    }
}
