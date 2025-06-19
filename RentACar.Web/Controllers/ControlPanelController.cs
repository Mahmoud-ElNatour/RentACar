using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RentACar.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Employee")]

    public class ControlPanelController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<ControlPanelController> _logger;

        public ControlPanelController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<ControlPanelController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet("~/ControlPanel")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("~/ControlPanel/ChangeRole")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult ChangeRole()
        {
            return View();
        }


        [HttpPost("ChangeRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDTO model)
        {
            _logger.LogInformation("Changing role for {User}", model.UserName);
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
