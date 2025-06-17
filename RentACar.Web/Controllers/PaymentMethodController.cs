using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;

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

        public PaymentMethodController(PaymentMethodManager paymentMethodManager,
                                       UserManager<IdentityUser> userManager,
                                       ILogger<PaymentMethodController> logger)
        {
            _paymentMethodManager = paymentMethodManager;
            _userManager = userManager;
            _logger = logger;
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
            if (method == null) return NotFound();
            return Ok(method);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaymentMethodDto>> Create([FromBody] PaymentMethodDto dto)
        {
            _logger.LogInformation("Creating payment method");
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var created = await _paymentMethodManager.AddPaymentMethodAsync(dto, userId);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentMethodDto dto)
        {
            if (id != dto.Id) return BadRequest();
            _logger.LogInformation("Updating payment method {Id}", id);
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var updated = await _paymentMethodManager.UpdatePaymentMethodAsync(dto, userId);
            if (updated == null) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting payment method {Id}", id);
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var success = await _paymentMethodManager.DeletePaymentMethodAsync(id, userId);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
