using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        private readonly PaymentManager _paymentManager;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(PaymentManager paymentManager, ILogger<PaymentController> logger)
        {
            _paymentManager = paymentManager;
            _logger = logger;
        }

        [HttpGet("~/Payment")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Views/ControlPanel/Payment/Index.cshtml");
        }

        [HttpGet("~/Payment/Add")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult AddView()
        {
            return View("~/Views/ControlPanel/Payment/Add.cshtml", new PaymentDto
            {
                PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow)
            });
        }

        [HttpGet("~/Payment/Edit/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditView(int id)
        {
            var payment = await _paymentManager.GetPaymentForEditAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            return View("~/Views/ControlPanel/Payment/Edit.cshtml", payment);
        }

        [HttpGet("~/Payment/Details/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DetailsView(int id)
        {
            var payment = await _paymentManager.GetPaymentDetailsByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            return View("~/Views/ControlPanel/Payment/Details.cshtml", payment);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDetailsDto>>> Get()
        {
            var payments = await _paymentManager.GetAllPaymentsWithDetailsAsync();
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDetailsDto>> Get(int id)
        {
            var payment = await _paymentManager.GetPaymentDetailsByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDto>> Create([FromBody] PaymentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await _paymentManager.AddPaymentAsync(dto);
            if (created == null)
            {
                _logger.LogWarning("Failed to create payment for booking {BookingId}", dto.BookingId);
                return BadRequest("Unable to create payment. Please verify booking and payment method.");
            }

            return CreatedAtAction(nameof(Get), new { id = created.PaymentId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentDto dto)
        {
            if (id != dto.PaymentId)
            {
                return BadRequest("ID mismatch");
            }

            var updated = await _paymentManager.UpdatePaymentAsync(dto);
            if (updated == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("~/Payment/Checkout/{bookingId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> CreateCheckoutSession(int bookingId)
        {
            try
            {
                // 🔹 Get booking amount from manager
                var sessionUrl = await _paymentManager.CreateStripeCheckoutSessionAsync(bookingId);

                if (string.IsNullOrEmpty(sessionUrl))
                {
                    return BadRequest("Unable to create Stripe session.");
                }

                return Redirect(sessionUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe checkout creation failed");
                return StatusCode(500, "Stripe error");
            }
        }

    }
}
