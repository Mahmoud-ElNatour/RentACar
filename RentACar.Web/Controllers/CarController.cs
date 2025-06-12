using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin")]
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

        public async Task<IActionResult> Index(string? name, int? categoryId, bool? available, int page = 1)
        {
            var cars = await _carRepository.SearchByFilterAsync(name, null, categoryId, available);
            var carDtos = cars.Select(c => _mapper.Map<CarDto>(c)).ToList();

            const int pageSize = 20;
            var totalItems = carDtos.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var items = carDtos.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Name = name;
            ViewBag.CategoryId = categoryId;
            ViewBag.Available = available;
            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "CategoryId", "Name");

            return View("~/Views/ControlPanel/Car/Index.cshtml", items);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            await PopulateCategories();
            return PartialView("~/Views/ControlPanel/Car/_CarFormPartial.cshtml", new CarDto { IsAvailable = true });
        }

        [HttpPost]
        public async Task<IActionResult> Add(CarDto dto)
        {
            if (ModelState.IsValid)
            {
                var entity = _mapper.Map<Car>(dto);
                await _carRepository.AddAsync(entity);
                return RedirectToAction(nameof(Index));
            }
            await PopulateCategories();
            return PartialView("~/Views/ControlPanel/Car/_CarFormPartial.cshtml", dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            await PopulateCategories();
            return PartialView("~/Views/ControlPanel/Car/_CarFormPartial.cshtml", _mapper.Map<CarDto>(car));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CarDto dto)
        {
            if (ModelState.IsValid)
            {
                var car = await _carRepository.GetByIdAsync(dto.CarId);
                if (car == null) return NotFound();

                _mapper.Map(dto, car);
                await _carRepository.UpdateAsync(car);
                return RedirectToAction(nameof(Index));
            }
            await PopulateCategories();
            return PartialView("~/Views/ControlPanel/Car/_CarFormPartial.cshtml", dto);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Car/_DeleteCarPartial.cshtml", _mapper.Map<CarDto>(car));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int carId)
        {
            await _carRepository.DeleteAsync(carId);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategories()
        {
            var cats = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(cats, "CategoryId", "Name");
        }
    }
}