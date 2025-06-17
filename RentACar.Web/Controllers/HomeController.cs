using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RentACar.Web.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using RentACar.Application.Managers;
using RentACar.Application.DTOs;

namespace RentACar.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CarManager _carManager;
        private readonly CategoryManager _categoryManager;
        private readonly CustomerManager _customerManager;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<IdentityUser> userManager,
            CarManager carManager,
            CategoryManager categoryManager,
            CustomerManager customerManager)
        {
            _logger = logger;
            _userManager = userManager;
            _carManager = carManager;
            _categoryManager = categoryManager;
            _customerManager = customerManager;
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

            var model = new HomeIndexViewModel
            {
                UserRole = userRole,
                AvailableCars = await _carManager.GetAvailableCarCountAsync(),
                TotalCustomers = await _customerManager.GetTotalCustomerCountAsync(),
                Categories = await _categoryManager.GetAllCategoriesAsync()
            };

            return View(model);
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
