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

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : Controller
    {
        private readonly CarManager _carManager;
        private readonly CategoryManager _categoryManager;

            public CarController(CarManager carManager, CategoryManager categoryManager)
            {
                _carManager = carManager;
                _categoryManager = categoryManager;
            }

        [HttpGet("~/Car")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Views/ControlPanel/Car/Index.cshtml");
        }

        [HttpGet("~/Car/Add")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> AddForm()
        {
            await PopulateCategories();
            return PartialView("~/Views/ControlPanel/Car/_CarFormPartial.cshtml", new CarDto { IsAvailable = true });
        }

        [HttpGet("~/Car/Edit/{id}")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditForm(int id)
        {
            var car = await _carManager.GetCarByIdAsync(id);
            if (car == null) return NotFound();
            await PopulateCategories();
            return PartialView("~/Views/ControlPanel/Car/_CarFormPartial.cshtml", car);
        }

        [HttpGet("~/Car/Delete/{id}")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var car = await _carManager.GetCarByIdAsync(id);
            if (car == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Car/_DeleteCarPartial.cshtml", car);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarDto>>> Get([FromQuery] string? name, [FromQuery] int? categoryId, [FromQuery] bool? available)
        {
            var carDtos = await _carManager.SearchCarsByFilterAsync(name, null, categoryId, available);
            return Ok(carDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CarDto>> Get(int id)
        {
            var car = await _carManager.GetCarByIdAsync(id);
            if (car == null) return NotFound();
            return Ok(car);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CarDto>> Create([FromBody] CarDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var createdCar = await _carManager.AddCarAsync(dto, userId);
            if (createdCar == null) return BadRequest("Unable to add car. Check if user is admin or plate number already exists.");

            return CreatedAtAction(nameof(Get), new { id = createdCar.CarId }, createdCar);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CarDto dto)
        {
            if (id != dto.CarId) return BadRequest();
            await _carManager.UpdateCarAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _carManager.DeleteCarAsync(id);
            return NoContent();
        }

        private async Task PopulateCategories()
        {
            var cats = await _categoryManager.GetAllCategoriesAsync();
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cats, "CategoryId", "Name");
        }
    }
}
