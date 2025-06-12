using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers
{
    public class ControlPanelController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ControlPanelController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangeRole()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userName, string role)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(role))
            {
                ModelState.AddModelError(string.Empty, "Username and role are required.");
                return View();
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View();
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                var createResult = await _roleManager.CreateAsync(new IdentityRole(role));
                if (!createResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Could not create role.");
                    return View();
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Could not remove existing roles.");
                return View();
            }

            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Could not assign role.");
                return View();
            }

            ViewBag.SuccessMessage = "Role updated successfully.";
            ModelState.Clear();
            return View();
        }
    }
}
