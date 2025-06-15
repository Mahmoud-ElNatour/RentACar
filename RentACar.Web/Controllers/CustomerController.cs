using System.Collections.Generic;
using System.Linq;
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
    public class CustomerController : Controller
    {
        private readonly CustomerManager _customerManager;
        private readonly IMapper _mapper;

        public CustomerController(CustomerManager customerManager, IMapper mapper)
        {
            _customerManager = customerManager;
            _mapper = mapper;
        }

        [HttpGet("~/Customer")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Views/ControlPanel/Customer/Index.cshtml");
        }

        [HttpGet("~/Customer/Add")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult AddForm()
        {
            return PartialView("~/Views/ControlPanel/Customer/_CustomerFormPartial.cshtml", new CustomerDTO { Isactive = true });
        }

        [HttpGet("~/Customer/Edit/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditForm(int id)
        {
            var customer = await _customerManager.GetCustomerById(id);
            if (customer == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Customer/_CustomerFormPartial.cshtml", customer);
        }

        [HttpGet("~/Customer/Delete/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var customer = await _customerManager.GetCustomerById(id);
            if (customer == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Customer/_DeleteCustomerPartial.cshtml", customer);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> Get([FromQuery] string? search, [FromQuery] bool? verified, [FromQuery] bool? active)
        {
            var customers = await _customerManager.GetAllCustomers();
            if (!string.IsNullOrEmpty(search))
            {
                customers = customers.Where(c => (c.Name != null && c.Name.Contains(search, System.StringComparison.OrdinalIgnoreCase)) || c.UserId.ToString() == search).ToList();
            }
            if (verified.HasValue)
                customers = customers.Where(c => c.IsVerified == verified.Value).ToList();
            if (active.HasValue)
                customers = customers.Where(c => c.Isactive == active.Value).ToList();
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDTO>> Get(int id)
        {
            var customer = await _customerManager.GetCustomerById(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDTO>> Create([FromBody] CustomerCreateDTO dto)
        {
            var created = await _customerManager.CreateCustomer(dto);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = created.UserId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerDTO dto)
        {
            if (id != dto.UserId) return BadRequest();
            await _customerManager.UpdateCustomer(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _customerManager.DeleteCustomer(id);
            return NoContent();
        }

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var success = await _customerManager.ResetPassword(id, "C@c123456");
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
