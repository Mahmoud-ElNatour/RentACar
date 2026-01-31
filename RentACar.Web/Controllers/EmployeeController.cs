using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
<<<<<<< HEAD
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
=======
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
>>>>>>> Mahmoud-V3
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Web.Controllers
{
<<<<<<< HEAD
    [Authorize(Roles = "Admin")]
=======
    [Authorize(Roles = "Admin,Employee")]
>>>>>>> Mahmoud-V3
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : Controller
    {
        private readonly EmployeeManager _employeeManager;
<<<<<<< HEAD
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(EmployeeManager employeeManager, IMapper mapper, ILogger<EmployeeController> logger)
        {
            _employeeManager = employeeManager;
            _mapper = mapper;
            _logger = logger;
=======
        private readonly ILogger<EmployeeController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EmployeeController(EmployeeManager employeeManager, RoleManager<IdentityRole> roleManager, ILogger<EmployeeController> logger)
        {
            _employeeManager = employeeManager;
            _logger = logger;
            _roleManager = roleManager;
>>>>>>> Mahmoud-V3
        }

        [HttpGet("~/Employee")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Views/ControlPanel/Employee/Index.cshtml");
        }

        [HttpGet("~/Employee/Add")]
<<<<<<< HEAD
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult AddForm()
        {
            return PartialView("~/Views/ControlPanel/Employee/_EmployeeFormPartial.cshtml", new EmployeeDto { IsActive = true });
        }

        [HttpGet("~/Employee/Edit/{id}")]
=======
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult AddForm()
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return PartialView("~/Views/ControlPanel/Employee/_EmployeeFormPartial.cshtml", new EmployeeDto { IsActive = true, DriverIsActive = true });
        }

        [HttpGet("~/Employee/Edit/{id}")]
        [Authorize(Roles = "Admin")]
>>>>>>> Mahmoud-V3
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditForm(int id)
        {
            var emp = await _employeeManager.GetEmployeeById(id);
            if (emp == null) return NotFound();
<<<<<<< HEAD
=======
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
>>>>>>> Mahmoud-V3
            return PartialView("~/Views/ControlPanel/Employee/_EmployeeFormPartial.cshtml", emp);
        }

        [HttpGet("~/Employee/Delete/{id}")]
<<<<<<< HEAD
=======
        [Authorize(Roles = "Admin")]
>>>>>>> Mahmoud-V3
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var emp = await _employeeManager.GetEmployeeById(id);
            if (emp == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Employee/_DeleteEmployeePartial.cshtml", emp);
        }

        [HttpGet]
<<<<<<< HEAD
        public async Task<ActionResult<PagedResultDto<EmployeeDisplayDto>>> Get(
            [FromQuery] string? search, 
            [FromQuery] bool? active, 
            [FromQuery] string? role,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortColumn = "Name",
            [FromQuery] string? sortDirection = "asc")
        {
            try
            {
                var result = await _employeeManager.GetEmployeesPagedAsync(search, active, role, page, pageSize, sortColumn, sortDirection);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load employees paged");
                return StatusCode(500, new { message = ex.Message });
            }
=======
        public async Task<ActionResult<IEnumerable<EmployeeDisplayDto>>> Get([FromQuery] string? search, [FromQuery] bool? active, [FromQuery] string? role)
        {
            var employees = await _employeeManager.GetAllEmployeesWithRoles();
            if (!string.IsNullOrWhiteSpace(search))
            {
                employees = employees.Where(e =>
                    (!string.IsNullOrEmpty(e.Name) && e.Name.Contains(search, System.StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(e.Email) && e.Email.Contains(search, System.StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(e.DriverCode) && e.DriverCode.Contains(search, System.StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            if (active.HasValue)
            {
                employees = employees.Where(e => e.IsActive == active.Value).ToList();
            }
            if (!string.IsNullOrWhiteSpace(role))
            {
                if (role.Equals("Employee", System.StringComparison.OrdinalIgnoreCase))
                {
                    employees = employees
                        .Where(e => e.Roles.Any(r => r.Equals("Employee", System.StringComparison.OrdinalIgnoreCase))
                                    && !e.Roles.Any(r => r.Equals("Driver", System.StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }
                else if (role.Equals("Driver", System.StringComparison.OrdinalIgnoreCase))
                {
                    employees = employees
                        .Where(e => e.Roles.Any(r => r.Equals("Driver", System.StringComparison.OrdinalIgnoreCase)) && e.DriverId.HasValue)
                        .ToList();
                }
                else
                {
                    employees = employees
                        .Where(e => e.Roles.Any(r => r.Equals(role, System.StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }
            }
            return Ok(employees);
>>>>>>> Mahmoud-V3
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> Get(int id)
        {
            var emp = await _employeeManager.GetEmployeeById(id);
            if (emp == null) return NotFound();
            return Ok(emp);
        }

        [HttpPost]
<<<<<<< HEAD
=======
        [Authorize(Roles = "Admin")]
>>>>>>> Mahmoud-V3
        public async Task<ActionResult<EmployeeDto>> Create([FromBody] EmployeeCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating employee");
            try
            {
                var created = await _employeeManager.CreateEmployee(dto);
                return CreatedAtAction(nameof(Get), new { id = created!.EmployeeId }, created);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create employee: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
<<<<<<< HEAD
=======
        [Authorize(Roles = "Admin")]
>>>>>>> Mahmoud-V3
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
            try
            {
                await _employeeManager.UpdateEmployee(dto);
                return NoContent(); // 204 success
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to update employee {Id}: {Message}", id, ex.Message);
                return Conflict(new { message = ex.Message });
            }
        }


        [HttpDelete("{id}")]
<<<<<<< HEAD
=======
        [Authorize(Roles = "Admin")]
>>>>>>> Mahmoud-V3
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting employee {Id}", id);
            try
            {
                await _employeeManager.DeleteEmployee(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Employee {Id} could not be deleted: {Message}", id, ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database constraint prevented deleting employee {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Unable to delete employee because related records exist. Remove the related data before deleting the employee.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting employee {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while deleting the employee. Please try again later.");
            }
        }

        [HttpPost("{id}/reset-password")]
<<<<<<< HEAD
        public async Task<IActionResult> ResetPassword(int id)
        {
            var success = await _employeeManager.ResetPassword(id);
=======
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var success = await _employeeManager.ResetPassword(id, "E@e123456");
>>>>>>> Mahmoud-V3
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
<<<<<<< HEAD

=======
>>>>>>> Mahmoud-V3
