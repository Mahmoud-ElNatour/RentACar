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
            DateOnly? endDate = null,
            int page = 1)
        {
            var categories = await _categoryManager.GetAllCategoriesAsync();

            var cars = await GetFilteredCarsInternal(name, categoryIds, minPrice, maxPrice, startDate, endDate);
            
            int pageSize = 12;
            int totalCount = cars.Count;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var pagedCars = cars.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var model = new BrowseViewDTO
            {
                Cars = pagedCars,
                Categories = categories,
                FilterName = name,
                FilterCategoryIds = categoryIds ?? Array.Empty<int>(),
                FilterMinPrice = minPrice,
                FilterMaxPrice = maxPrice,
                FilterStartDate = startDate,
                FilterEndDate = endDate,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(model);
        }

        [HttpGet("~/Browse/LoadMore")]
        public async Task<IActionResult> LoadMore(
            string? name = null,
            [FromQuery] int[]? categoryIds = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            int page = 1)
        {
            var cars = await GetFilteredCarsInternal(name, categoryIds, minPrice, maxPrice, startDate, endDate);

            int pageSize = 12;
            var pagedCars = cars.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            if (!pagedCars.Any()) return NoContent();

            return PartialView("_CarGridItems", pagedCars);
        }

        private async Task<List<CarListDto>> GetFilteredCarsInternal(
            string? name, 
            int[]? categoryIds, 
            decimal? minPrice, 
            decimal? maxPrice, 
            DateOnly? startDate, 
            DateOnly? endDate)
        {
            // Initial Fetch (Lightweight DTOs)
            var cars = await _carManager.SearchCarsForListAsync(modelName: name);

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

            // Apply Date Availability Logic
            if (startDate.HasValue || endDate.HasValue)
            {
                var start = startDate ?? endDate ?? DateOnly.FromDateTime(DateTime.Today);
                var end = endDate ?? start;
                if (end < start) (start, end) = (end, start);
                
                // Fetch unavailable cars in timeline (or available ones)
                // Manager returns *Available* cars.
                // We must intersect with the search result.
                var available = await _carManager.GetAvailableCarsInTimelineAsync(start.ToDateTime(TimeOnly.MinValue), end.ToDateTime(TimeOnly.MinValue));
                var availIds = available.Select(c => c.CarId).ToHashSet();
                
                cars = cars.Where(c => availIds.Contains(c.CarId)).ToList();
            }

            return cars;
        }

    }
}
