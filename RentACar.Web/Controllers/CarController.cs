using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : Controller
    {
        private readonly ICarRepository _carRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CarController(ICarRepository carRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _carRepository = carRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
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
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null) return NotFound();
            await PopulateCategories();
            return PartialView("~/Views/ControlPanel/Car/_CarFormPartial.cshtml", _mapper.Map<CarDto>(car));
        }

        [HttpGet("~/Car/Delete/{id}")]
        [Authorize(Roles = "Admin")]

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Car/_DeleteCarPartial.cshtml", _mapper.Map<CarDto>(car));
        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<CarDto>>> Get([FromQuery] string? name, [FromQuery] int? categoryId, [FromQuery] bool? available)
        {
            var cars = await _carRepository.SearchByFilterAsync(name, null, categoryId, available);
            var carDtos = cars.Select(c => _mapper.Map<CarDto>(c)).ToList();
            return Ok(carDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CarDto>> Get(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null) return NotFound();
            return Ok(_mapper.Map<CarDto>(car));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CarDto>> Create([FromBody] CarDto dto)
        {
            var entity = _mapper.Map<Car>(dto);
            await _carRepository.AddAsync(entity);
            var result = _mapper.Map<CarDto>(entity);
            return CreatedAtAction(nameof(Get), new { id = result.CarId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CarDto dto)
        {
            if (id != dto.CarId) return BadRequest();
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null) return NotFound();
            _mapper.Map(dto, car);
            await _carRepository.UpdateAsync(car);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _carRepository.DeleteAsync(id);
            return NoContent();
        }

        private async Task PopulateCategories()
        {
            var cats = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cats, "CategoryId", "Name");
        }
    }
}