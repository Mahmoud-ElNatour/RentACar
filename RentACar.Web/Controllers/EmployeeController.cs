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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<EmployeeDto>> Create([FromBody] EmployeeCreateDTO dto)
        {
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

            await _employeeManager.UpdateEmployee(dto);
            return NoContent(); // 204 success
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