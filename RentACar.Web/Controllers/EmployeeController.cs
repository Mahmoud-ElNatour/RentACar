using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using Microsoft.Extensions.Logging;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : Controller
    {
        private readonly EmployeeManager _employeeManager;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(EmployeeManager employeeManager, IMapper mapper, ILogger<EmployeeController> logger)
        {
            _employeeManager = employeeManager;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("~/Employee")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Views/ControlPanel/Employee/Index.cshtml");
        }

        [HttpGet("~/Employee/Add")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult AddForm()
        {
            return PartialView("~/Views/ControlPanel/Employee/_EmployeeFormPartial.cshtml", new EmployeeDto { IsActive = true });
        }

        [HttpGet("~/Employee/Edit/{id}")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditForm(int id)
        {
            var emp = await _employeeManager.GetEmployeeById(id);
            if (emp == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Employee/_EmployeeFormPartial.cshtml", emp);
        }

        [HttpGet("~/Employee/Delete/{id}")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var emp = await _employeeManager.GetEmployeeById(id);
            if (emp == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Employee/_DeleteEmployeePartial.cshtml", emp);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDisplayDto>>> Get([FromQuery] string? search, [FromQuery] bool? active, [FromQuery] string? role)
        {
            var employees = await _employeeManager.GetAllEmployeesWithRoles();
            if (!string.IsNullOrWhiteSpace(search))
            {
                employees = employees.Where(e =>
                    (!string.IsNullOrEmpty(e.Name) && e.Name.Contains(search, System.StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(e.Email) && e.Email.Contains(search, System.StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            if (active.HasValue)
            {
                employees = employees.Where(e => e.IsActive == active.Value).ToList();
            }
            if (!string.IsNullOrWhiteSpace(role))
            {
                employees = employees.Where(e => string.Equals(e.Role, role, System.StringComparison.OrdinalIgnoreCase)).ToList();
            }
            return Ok(employees);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> Get(int id)
        {
            var emp = await _employeeManager.GetEmployeeById(id);
            if (emp == null) return NotFound();
            return Ok(emp);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<EmployeeDto>> Create([FromBody] EmployeeCreateDTO dto)
        {
            _logger.LogInformation("Creating employee");
            var created = await _employeeManager.CreateEmployee(dto);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = created.EmployeeId }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeDto dto)
        {
            // ✅ Check if the model is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Will return a 400 with explanation of what's wrong
            }

            // ✅ Ensure the ID in URL matches the body
            if (id != dto.EmployeeId)
            {
                return BadRequest("Employee ID mismatch.");
            }

            _logger.LogInformation("Updating employee {Id}", id);

            await _employeeManager.UpdateEmployee(dto);
            return NoContent(); // 204 success
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting employee {Id}", id);
            await _employeeManager.DeleteEmployee(id);
            return NoContent();
        }
    }
}