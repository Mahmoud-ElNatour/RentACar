using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Core.Managers;

namespace RentACar.Web.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Employee")]
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        private readonly PaymentManager _paymentManager;
        private readonly PaymentMethodManager _paymentMethodManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(PaymentManager paymentManager,
                                 PaymentMethodManager paymentMethodManager,
                                 UserManager<IdentityUser> userManager,
                                 ILogger<PaymentController> logger)
        {
            _paymentManager = paymentManager;
            _paymentMethodManager = paymentMethodManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("~/Payment")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult IndexView()
        {
            return View("~/Views/ControlPanel/Payment/Index.cshtml");
        }


        [HttpGet("~/Payment/Edit/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditView(int id)
        {
            var payment = await _paymentManager.GetPaymentByIdAsync(id);
            if (payment == null) return NotFound();
            ViewBag.Methods = await _paymentMethodManager.GetAllPaymentMethodsAsync();
            return View("~/Views/ControlPanel/Payment/Edit.cshtml", payment);
        }

        [HttpGet("~/Payment/Delete/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteView(int id)
        {
            var payment = await _paymentManager.GetPaymentByIdAsync(id);
            if (payment == null) return NotFound();
            return View("~/Views/ControlPanel/Payment/Delete.cshtml", payment);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> Get()
        {
            var list = await _paymentManager.GetAllPaymentsAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> Get(int id)
        {
            var payment = await _paymentManager.GetPaymentByIdAsync(id);
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDto>> Create([FromBody] PaymentDto dto)
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var created = await _paymentManager.AddPaymentAsync(dto, userId);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = created.PaymentId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentDto dto)
        {
            if (id != dto.PaymentId) return BadRequest();
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var updated = await _paymentManager.UpdatePaymentAsync(dto, userId);
            if (updated == null) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var success = await _paymentManager.DeletePaymentAsync(id, userId);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}