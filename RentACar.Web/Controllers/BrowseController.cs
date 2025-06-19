using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Core.Entities;
using System.Linq;

namespace RentACar.Web.Controllers
{
    public class BrowseController : Controller
    {

        private readonly CarManager _carManager;
        private readonly CategoryManager _categoryManager;

        public BrowseController(CarManager carManager, CategoryManager categoryManager)
        {
            _carManager = carManager;
            _categoryManager = categoryManager;
        }

        [HttpGet("~/Browse")]
        public async Task<IActionResult> Index(string? name = null, int? categoryId = null, decimal? maxPrice = null, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var categories = await _categoryManager.GetAllCategoriesAsync();

            // Get all filtered cars (without price yet)
            var cars = await _carManager.SearchCarsByFilterAsync(modelName: name, categoryId: categoryId);

            // Apply price filter here (client-side filter, unless you want to add price param to CarManager too)
            if (maxPrice.HasValue)
            {
                cars = cars.Where(c => c.PricePerDay <= maxPrice.Value).ToList();
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                var available = await _carManager.GetAvailableCarsInTimelineAsync(startDate.Value.ToDateTime(TimeOnly.MinValue), endDate.Value.ToDateTime(TimeOnly.MinValue));
                var availIds = available.Select(c => c.CarId).ToHashSet();
                cars = cars.Where(c => availIds.Contains(c.CarId)).ToList();
            }

            var model = new BrowseViewDTO
            {
                Cars = cars,
                Categories = categories,
                FilterName = name,
                FilterCategoryId = categoryId,
                FilterMaxPrice = maxPrice,
                FilterStartDate = startDate,
                FilterEndDate = endDate
            };

            return View(model);
        }

    }
}
