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
        public async Task<IActionResult> Index(
            string? name = null, 
            [FromQuery] int[]? categoryIds = null, 
            decimal? minPrice = null, 
            decimal? maxPrice = null, 
            DateOnly? startDate = null, 
            DateOnly? endDate = null)
        {
            var categories = await _categoryManager.GetAllCategoriesAsync();

            // Get all filtered cars (without price yet)
            // Passing null for categoryId since we handle multiple below or client side
            // Ideally we'd pass the array to manager, but for now we filter in-memory as before or adjust manager call.
            // Since SearchCarsByFilterAsync only takes single ID, let's fetch all (filtered by name) and filter manually.
            var cars = await _carManager.SearchCarsByFilterAsync(modelName: name);

            // Filter by Categories
            if (categoryIds != null && categoryIds.Any())
            {
                cars = cars.Where(c => c.CategoryId.HasValue && categoryIds.Contains(c.CategoryId.Value)).ToList();
            }

            // Apply price filter
            if (minPrice.HasValue)
            {
                cars = cars.Where(c => c.PricePerDay >= minPrice.Value).ToList();
            }
            if (maxPrice.HasValue)
            {
                cars = cars.Where(c => c.PricePerDay <= maxPrice.Value).ToList();
            }

            if (startDate.HasValue || endDate.HasValue)
            {
                var start = startDate ?? endDate ?? DateOnly.FromDateTime(DateTime.Today);
                var end = endDate ?? start;
                if (end < start)
                {
                    var temp = start;
                    start = end;
                    end = temp;
                }
                var available = await _carManager.GetAvailableCarsInTimelineAsync(start.ToDateTime(TimeOnly.MinValue), end.ToDateTime(TimeOnly.MinValue));
                var availIds = available.Select(c => c.CarId).ToHashSet();
                cars = cars.Where(c => availIds.Contains(c.CarId)).ToList();
            }

            var model = new BrowseViewDTO
            {
                Cars = cars,
                Categories = categories,
                FilterName = name,
                FilterCategoryIds = categoryIds ?? Array.Empty<int>(),
                FilterMinPrice = minPrice,
                FilterMaxPrice = maxPrice,
                FilterStartDate = startDate,
                FilterEndDate = endDate
            };

            return View(model);
        }

    }
}
