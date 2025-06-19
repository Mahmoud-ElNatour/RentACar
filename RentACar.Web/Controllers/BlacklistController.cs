using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Core.Repositories;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [ApiController]
    [Route("api/[controller]")]
    public class BlacklistController : Controller
    {
        private readonly BlacklistManager _blacklistManager;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<BlacklistController> _logger;

        public BlacklistController(BlacklistManager blacklistManager, IEmployeeRepository employeeRepository, UserManager<IdentityUser> userManager, ILogger<BlacklistController> logger)
        {
            _blacklistManager = blacklistManager;
            _employeeRepository = employeeRepository;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("~/Blacklist")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Views/ControlPanel/Blacklist/Index.cshtml");
        }

        [HttpGet("~/Blacklist/Add")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult AddForm()
        {
            return PartialView("~/Views/ControlPanel/Blacklist/_AddBlacklistPartial.cshtml", new AddToBlacklistRequestDto());
        }

        [HttpGet("~/Blacklist/Edit/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditForm(int id)
        {
            var bl = await _blacklistManager.GetByIdAsync(id);
            if (bl == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Blacklist/_EditBlacklistPartial.cshtml", bl);
        }

        [HttpGet("~/Blacklist/Delete/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var bl = await _blacklistManager.GetByIdAsync(id);
            if (bl == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Blacklist/_DeleteBlacklistPartial.cshtml", bl);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlacklistDisplayDto>>> Get([FromQuery] string? type, [FromQuery] string? search, [FromQuery] int? offset)
        {
            var list = await _blacklistManager.GetAllAsync(type, search, offset ?? 0);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BlacklistDto>> Get(int id)
        {
            var bl = await _blacklistManager.GetByIdAsync(id);
            if (bl == null) return NotFound();
            return Ok(bl);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddToBlacklistRequestDto dto)
        {
            _logger.LogInformation("Creating blacklist entry for {Identifier}", dto.Identifier);
            var emp = await GetLoggedEmployee();
            if (emp == null) return Unauthorized();
            var result = await _blacklistManager.AddToBlacklistAsync(dto, emp);
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message, data = result.Data });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BlacklistDto dto)
        {
            if (id != dto.BlacklistId) return BadRequest();
            await _blacklistManager.UpdateBlacklistAsync(dto);
            return Ok(new { message = "Done" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting blacklist entry {Id}", id);
            var emp = await GetLoggedEmployee();
            if (emp == null) return Unauthorized();
            var result = await _blacklistManager.RemoveByIdAsync(id, emp);
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

        private async Task<EmployeeDto?> GetLoggedEmployee()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return null;
            var empEntity = await _employeeRepository.GetByIdAsync(userId);
            if (empEntity == null) return null;
            return new EmployeeDto
            {
                EmployeeId = empEntity.EmployeeId,
                Name = empEntity.Name,
                Salary = empEntity.Salary,
                Address = empEntity.Address,
                IsActive = empEntity.IsActive,
                Email = empEntity.User.Email,
                username = empEntity.User.UserName,
                PhoneNumber = empEntity.User.PhoneNumber,
                aspNetUserId = empEntity.aspNetUserId
            };
        }
    }
}
