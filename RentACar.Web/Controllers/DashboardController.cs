using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRole = roles.FirstOrDefault() ?? "Customer";

            return View();
        }
    }
}
