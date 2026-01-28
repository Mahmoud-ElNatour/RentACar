using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using RentACar.Application.Managers;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class RatingsManagementController : Controller
    {
        private readonly CustomerRatingManager _ratingManager;

        public RatingsManagementController(CustomerRatingManager ratingManager)
        {
            _ratingManager = ratingManager;
        }

        [HttpGet("~/ControlPanel/Ratings")]
        public async Task<IActionResult> Index(string? searchTerm, string? sortColumn, string? sortDirection)
        {
            ViewBag.CurrentSort = sortColumn;
            ViewBag.CurrentSortDir = sortDirection;
            ViewBag.CurrentSearch = searchTerm;

            // Default sort direction logic for UI toggling
            ViewBag.NextSortDir = sortDirection == "asc" ? "desc" : "asc";

            var ratings = await _ratingManager.GetAllRatingsAsync(searchTerm, sortColumn, sortDirection);
            
            // Calculate basic stats for the view
            ViewBag.TotalRatings = ratings.Count;
            ViewBag.AverageRating = ratings.Any() ? ratings.Average(r => r.Stars) : 0;
            
            return View("~/Views/ControlPanel/Ratings/Index.cshtml", ratings);
        }

        [HttpGet("~/ControlPanel/Ratings/Export")]
        public async Task<IActionResult> Export(string? searchTerm, string? sortColumn, string? sortDirection)
        {
            var ratings = await _ratingManager.GetAllRatingsAsync(searchTerm, sortColumn, sortDirection);
            
            var csvConfig = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };

            using var memoryStream = new System.IO.MemoryStream();
            using var streamWriter = new System.IO.StreamWriter(memoryStream);
            using var csvWriter = new CsvHelper.CsvWriter(streamWriter, csvConfig);

            // Write custom simplified model for export
            var exportList = ratings.Select(r => new 
            {
                r.BookingId,
                Date = r.RatingDate.ToString("yyyy-MM-dd"),
                Customer = r.CustomerName,
                Email = r.CustomerEmail,
                r.Stars,
                Feedback = r.Feedback?.Replace("\n", " ") ?? ""
            }).ToList();

            await csvWriter.WriteRecordsAsync(exportList);
            await streamWriter.FlushAsync();
            
            return File(memoryStream.ToArray(), "text/csv", $"Ratings_Export_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        [HttpGet("~/ControlPanel/Ratings/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var details = await _ratingManager.GetFullRatingDetailsAsync(id);
            if (details == null)
            {
                return NotFound();
            }

            return PartialView("~/Views/ControlPanel/Ratings/_RatingDetails.cshtml", details);
        }
    }
}
