using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RentACar.Web.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using RentACar.Application.Managers;

namespace RentACar.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CarManager _carManager;
        private readonly CategoryManager _categoryManager;

        public HomeController(ILogger<HomeController> logger,
                              UserManager<IdentityUser> userManager,
                              CarManager carManager,
                              CategoryManager categoryManager)
        {
            _logger = logger;
            _userManager = userManager;
            _carManager = carManager;
            _categoryManager = categoryManager;
        }

        public async Task<IActionResult> Index()
        {
            string userRole = string.Empty;

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userRole = roles.FirstOrDefault() ?? "No Role";
                }
            }

            var cars = await _carManager.BrowseAllCarsAsync();
            var categories = await _categoryManager.GetAllCategoriesAsync();

            ViewBag.UserRole = userRole;
            ViewBag.CarsCount = cars.Count;
            ViewBag.Categories = categories;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
