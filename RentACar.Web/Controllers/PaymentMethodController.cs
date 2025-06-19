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
    public class PaymentMethodController : Controller
    {
        private readonly PaymentMethodManager _paymentMethodManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<PaymentMethodController> _logger;

        public PaymentMethodController(
            PaymentMethodManager paymentMethodManager,
            UserManager<IdentityUser> userManager,
            ILogger<PaymentMethodController> logger)
        {
            _paymentMethodManager = paymentMethodManager;
            _userManager = userManager;
            _logger = logger;
        }
        // Return the table view for listing
        [HttpGet("~/PaymentMethod")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult IndexView()
        {
            return View("~/Views/ControlPanel/PaymentMethod/Index.cshtml");
        }

        // Return the add form view
        [HttpGet("~/PaymentMethod/Add")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult AddView()
        {
            return View("~/Views/ControlPanel/PaymentMethod/Add.cshtml");
        }

        // Return the edit form view with model bound
        [HttpGet("~/PaymentMethod/Edit/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditView(int id)
        {
            var method = await _paymentMethodManager.GetPaymentMethodByIdAsync(id);
            if (method == null) return NotFound();
            return View("~/Views/ControlPanel/PaymentMethod/Edit.cshtml", method);
        }

        // Return the delete confirmation view with model bound
        [HttpGet("~/PaymentMethod/Delete/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteView(int id)
        {
            var method = await _paymentMethodManager.GetPaymentMethodByIdAsync(id);
            if (method == null) return NotFound();
            return View("~/Views/ControlPanel/PaymentMethod/Delete.cshtml", method);
        }



        [HttpGet]

        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> Get()
        {
            var methods = await _paymentMethodManager.GetAllPaymentMethodsAsync();
            return Ok(methods);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentMethodDto>> Get(int id)
        {
            var method = await _paymentMethodManager.GetPaymentMethodByIdAsync(id);
            if (method == null)
            {
                _logger.LogWarning("Payment method with ID {Id} not found", id);
                return NotFound();
            }

            return Ok(method);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaymentMethodDto>> Add([FromBody] PaymentMethodDto dto)
        {
            _logger.LogInformation("Creating payment method {Name}", dto.PaymentMethodName);
            var userId = _userManager.GetUserId(User) ?? string.Empty;

            var created = await _paymentMethodManager.AddPaymentMethodAsync(dto, userId);
            if (created == null)
            {
                _logger.LogWarning("Failed to create payment method {Name}", dto.PaymentMethodName);
                return BadRequest("Creation failed: method may already exist or user unauthorized.");
            }

            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentMethodDto dto)
        {
            if (id != dto.Id)
            {
                _logger.LogWarning("ID mismatch in update: route ID = {RouteId}, DTO ID = {DtoId}", id, dto.Id);
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating payment method {Id}", id);
            var userId = _userManager.GetUserId(User) ?? string.Empty;

            var updated = await _paymentMethodManager.UpdatePaymentMethodAsync(dto, userId);
            if (updated == null)
            {
                _logger.LogWarning("Payment method update failed for ID {Id}", id);
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting payment method {Id}", id);
            var userId = _userManager.GetUserId(User) ?? string.Empty;

            var success = await _paymentMethodManager.DeletePaymentMethodAsync(id, userId);
            if (!success)
            {
                _logger.LogWarning("Failed to delete payment method with ID {Id}", id);
                return NotFound();
            }

            return NoContent();
        }
    }
}
