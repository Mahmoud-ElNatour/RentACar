using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin")]
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
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult AddForm()
        {
            return PartialView("~/Views/ControlPanel/Employee/_EmployeeFormPartial.cshtml", new EmployeeDto { IsActive = true });
        }

        [HttpGet("~/Employee/Edit/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditForm(int id)
        {
            var emp = await _employeeManager.GetEmployeeById(id);
            if (emp == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Employee/_EmployeeFormPartial.cshtml", emp);
        }

        [HttpGet("~/Employee/Delete/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var emp = await _employeeManager.GetEmployeeById(id);
            if (emp == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Employee/_DeleteEmployeePartial.cshtml", emp);
        }

        [HttpGet]
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
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> Get(int id)
        {
            var emp = await _employeeManager.GetEmployeeById(id);
            if (emp == null) return NotFound();
            return Ok(emp);
        }

        [HttpPost]
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
        public async Task<IActionResult> ResetPassword(int id)
        {
            var success = await _employeeManager.ResetPassword(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}

