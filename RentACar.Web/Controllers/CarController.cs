using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : Controller
    {
        private readonly CarManager _carManager;
        private readonly CategoryManager _categoryManager;
        private readonly ILogger<CarController> _logger;

        public CarController(CarManager carManager, CategoryManager categoryManager, ILogger<CarController> logger)
        {
            _carManager = carManager;
            _categoryManager = categoryManager;
            _logger = logger;
        }

        [HttpGet("~/Car")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            _logger.LogInformation("🧭 [Index] Rendered Car Management Index View");
            return View("~/Views/ControlPanel/Car/Index.cshtml");
        }

        [HttpGet("~/Car/Add")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> AddForm()
        {
            _logger.LogInformation("📄 [AddForm] Loading Add Car Form");
            await PopulateCategories();
            return PartialView("~/Views/ControlPanel/Car/_CarFormPartial.cshtml", new CarDto { IsAvailable = true });
        }

        [HttpGet("~/Car/Edit/{id}")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditForm(int id)
        {
            _logger.LogInformation("✏️ [EditForm] Fetching car for edit with ID {Id}", id);
            var car = await _carManager.GetCarByIdAsync(id);
            if (car == null)
            {
                _logger.LogWarning("❌ [EditForm] Car with ID {Id} not found", id);
                return NotFound();
            }

            await PopulateCategories();
            return PartialView("~/Views/ControlPanel/Car/_CarFormPartial.cshtml", car);
        }

        [HttpGet("~/Car/Delete/{id}")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteForm(int id)
        {
            _logger.LogInformation("🗑️ [DeleteForm] Preparing delete form for Car ID {Id}", id);
            var car = await _carManager.GetCarByIdAsync(id);
            if (car == null)
            {
                _logger.LogWarning("❌ [DeleteForm] Car with ID {Id} not found", id);
                return NotFound();
            }

            return PartialView("~/Views/ControlPanel/Car/_DeleteCarPartial.cshtml", car);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarDto>>> Get([FromQuery] string? name, [FromQuery] int? categoryId, [FromQuery] bool? available)
        {
            _logger.LogInformation("📥 [Get] Querying cars - Name: {Name}, CategoryId: {CategoryId}, Available: {Available}", name, categoryId, available);
            var carDtos = await _carManager.SearchCarsByFilterAsync(name, null, categoryId, available);
            _logger.LogInformation("📦 [Get] Retrieved {Count} cars", carDtos.Count);
            return Ok(carDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CarDto>> Get(int id)
        {
            _logger.LogInformation("🔍 [GetById] Fetching car with ID {Id}", id);
            var car = await _carManager.GetCarByIdAsync(id);
            if (car == null)
            {
                _logger.LogWarning("❌ [GetById] Car with ID {Id} not found", id);
                return NotFound();
            }

            _logger.LogInformation("✅ [GetById] Found car: {@Car}", car);
            return Ok(car);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CarDto>> Create([FromBody] CarDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("🚫 [Create] Unauthorized access attempt");
                return Unauthorized();
            }

            _logger.LogInformation("🚗 [Create] Attempting to create car by user {UserId}. Car Data: {@CarDto}", userId, dto);
            var createdCar = await _carManager.AddCarAsync(dto, userId);
            if (createdCar == null)
            {
                _logger.LogWarning("❌ [Create] Car creation failed by user {UserId}. Possibly duplicate plate or no admin role", userId);
                return BadRequest("Unable to add car. Check if user is admin or plate number already exists.");
            }

            _logger.LogInformation("✅ [Create] Car created with ID {Id}", createdCar.CarId);
            return CreatedAtAction(nameof(Get), new { id = createdCar.CarId }, createdCar);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CarDto dto)
        {
            _logger.LogInformation("✏️ [Update] Updating car with ID {Id}", id);

            if (id != dto.CarId)
            {
                _logger.LogWarning("⚠️ [Update] Mismatch between route ID ({Id}) and payload ID ({PayloadId})", id, dto.CarId);
                return BadRequest("ID mismatch");
            }

            await _carManager.UpdateCarAsync(dto);
            _logger.LogInformation("✅ [Update] Car ID {Id} updated successfully", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("🗑️ [Delete] Request to delete car ID {Id}", id);
            await _carManager.DeleteCarAsync(id);
            _logger.LogInformation("✅ [Delete] Car ID {Id} deleted successfully", id);
            return NoContent();
        }

        private async Task PopulateCategories()
        {
            _logger.LogInformation("📋 [PopulateCategories] Loading categories for dropdown");
            var cats = await _categoryManager.GetAllCategoriesAsync();
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cats, "CategoryId", "Name");
        }

        [HttpGet("browseall")]
        public async Task<ActionResult<List<CarDto>>> BrowseAllCars()
        {
            var cars = await _carManager.BrowseAllCarsAsync();
            return Ok(cars);
        }

    }

}
