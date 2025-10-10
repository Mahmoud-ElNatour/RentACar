using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : Controller
    {
        private readonly CustomerManager _customerManager;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(CustomerManager customerManager, IMapper mapper, ILogger<CustomerController> logger)
        {
            _customerManager = customerManager;
            _mapper = mapper;
            _logger = logger;
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

        [HttpGet("~/Customer/Documents/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DocumentsForm(int id)
        {
            var customer = await _customerManager.GetCustomerById(id);
            if (customer == null) return NotFound();
            return PartialView("~/Views/ControlPanel/Customer/_CustomerDocumentsPartial.cshtml", customer);
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating customer");

            try
            {
                var created = await _customerManager.CreateCustomer(dto);
                return CreatedAtAction(nameof(Get), new { id = created!.UserId }, created);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create customer: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerDTO dto)
        {
            if (id != dto.UserId) return BadRequest();
            _logger.LogInformation("Updating customer {Id}", id);
            try
            {
                await _customerManager.UpdateCustomer(dto);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to update customer {Id}: {Message}", id, ex.Message);
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting customer {Id}", id);

            try
            {
                await _customerManager.DeleteCustomer(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Customer {Id} could not be deleted: {Message}", id, ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database constraint prevented deleting customer {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Unable to delete customer because related records exist. Remove the related data before deleting the customer.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting customer {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while deleting the customer. Please try again later.");
            }
        }

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var success = await _customerManager.ResetPassword(id, "C@c123456");
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/documents")]
        public async Task<IActionResult> UpdateDocuments(int id, [FromBody] CustomerDocumentsDto dto)
        {
            await _customerManager.UpdateCustomerDocuments(id, dto);
            return NoContent();
        }
    }
}
