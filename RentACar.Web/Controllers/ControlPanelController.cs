using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Web.Models;
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
        public async Task<IActionResult> ChangeRole(ChangeRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                var createResult = await _roleManager.CreateAsync(new IdentityRole(model.Role));
                if (!createResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Could not create role.");
                    return View(model);
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Could not remove existing roles.");
                return View(model);
            }

            var addResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Could not assign role.");
                return View(model);
            }

            ViewBag.SuccessMessage = "Role updated successfully.";
            ModelState.Clear();
            return View();
        }
    }
}
