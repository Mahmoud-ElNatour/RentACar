using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Web.Models;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControlPanelController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ControlPanelController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("/ControlPanel")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/ControlPanel/ChangeRole")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult ChangeRole()
        {
            return View();
        }

        [HttpPost("ChangeRole")]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                var createResult = await _roleManager.CreateAsync(new IdentityRole(model.Role));
                if (!createResult.Succeeded)
                {
                    return BadRequest(new { message = "Could not create role." });
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return BadRequest(new { message = "Could not remove existing roles." });
            }

            var addResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addResult.Succeeded)
            {
                return BadRequest(new { message = "Could not assign role." });
            }

            return Ok(new { message = "Role updated successfully." });
        }
    }
}
