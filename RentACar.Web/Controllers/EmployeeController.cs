using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : Controller
    {
        private readonly EmployeeManager _employeeManager;
        private readonly IMapper _mapper;

        public EmployeeController(EmployeeManager employeeManager, IMapper mapper)
        {
            _employeeManager = employeeManager;
            _mapper = mapper;
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
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> Get()
        {
            var employees = await _employeeManager.GetAllEmployees();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> Get(int id)
        {
            var emp = await _employeeManager.GetEmployeeById(id);
            if (emp == null) return NotFound();
            return Ok(emp);
        }

        private class EmployeeCreateRequest
        {
            public string Name { get; set; }
            public decimal? Salary { get; set; }
            public string Address { get; set; }
            public string Email { get; set; }
            public string Username { get; set; }
            public string PhoneNumber { get; set; }
            public string Password { get; set; }
            public bool IsActive { get; set; }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<EmployeeDto>> Create([FromBody] EmployeeCreateRequest request)
        {
            var dto = new EmployeeDto
            {
                Name = request.Name,
                Salary = request.Salary,
                Address = request.Address,
                Email = request.Email,
                username = request.Username,
                PhoneNumber = request.PhoneNumber,
                IsActive = request.IsActive
            };
            var created = await _employeeManager.CreateEmployee(dto, request.Password);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = created.EmployeeId }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeDto dto)
        {
            if (id != dto.EmployeeId) return BadRequest();
            await _employeeManager.UpdateEmployee(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _employeeManager.DeleteEmployee(id);
            return NoContent();
        }
    }
}
