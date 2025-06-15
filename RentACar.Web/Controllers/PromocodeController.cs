using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Employee")]
    [Route("api/[controller]")]
    public class PromocodeController : Controller
    {
        private readonly PromocodeManager _promocodeManager;
        private readonly UserManager<IdentityUser> _userManager;

        public PromocodeController(PromocodeManager promocodeManager, UserManager<IdentityUser> userManager)
        {
            _promocodeManager = promocodeManager;
            _userManager = userManager;
        }

        [HttpGet("~/Promocode")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Views/ControlPanel/Promocode/Index.cshtml");
        }

        [HttpGet("~/Promocode/Add")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult AddForm()
        {
            return PartialView("~/Views/ControlPanel/Promocode/_AddPromocode.cshtml", new PromocodeDto { IsActive = true });
        }

        [HttpGet("~/Promocode/Edit/{id}")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditForm(int id)
        {
            var promo = await _promocodeManager.GetPromocodeByIdAsync(id);
            if (promo == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Promocode/_EditPromocode.cshtml", promo);
        }

        [HttpGet("~/Promocode/Delete/{id}")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var promo = await _promocodeManager.GetPromocodeByIdAsync(id);
            if (promo == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Promocode/_DeletePromocode.cshtml", promo);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromocodeDto>>> Get()
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var promos = await _promocodeManager.GetAllPromocodesAsync(userId);
            return Ok(promos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PromocodeDto>> Get(int id)
        {
            var promo = await _promocodeManager.GetPromocodeByIdAsync(id);
            if (promo == null) return NotFound();
            return Ok(promo);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PromocodeDto>> Create([FromBody] PromocodeDto dto)
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var created = await _promocodeManager.AddPromocodeAsync(dto, userId);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = created.PromocodeId }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] PromocodeDto dto)
        {
            if (id != dto.PromocodeId) return BadRequest();
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var updated = await _promocodeManager.UpdatePromocodeAsync(dto, userId);
            if (updated == null) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var success = await _promocodeManager.DeletePromocodeAsync(id, userId);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
