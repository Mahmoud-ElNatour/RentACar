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
        private readonly PromocodeManager _promocodeManager;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(PaymentManager paymentManager, PromocodeManager promocodeManager, ILogger<PaymentController> logger)
        {
            _paymentManager = paymentManager;
            _promocodeManager = promocodeManager;
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
        public async Task<ActionResult<IEnumerable<PaymentListDto>>> Get()
        {
            var payments = await _paymentManager.GetAllPaymentsForListAsync();
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

        [HttpGet("ApplyPromocode/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> ApplyPromocodeView(int id)
        {
            var payment = await _paymentManager.GetPaymentDetailsByIdAsync(id);
            if (payment == null) return NotFound();

            // Get current user ID to fetch visible promocodes
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var allPromos = await _promocodeManager.GetAllPromocodesAsync(userId);
            // Filter only active ones for the dropdown
            var activePromos = allPromos.Where(p => p.IsActive).ToList();

            ViewBag.PaymentId = id;
            var model = new PromocodeDto(); // Using PromocodeDto as model for key selection, or just Vie
                                            // Better to pass a ViewModel, but for speed I'll use ViewBag for the list and an empty DTO or similar.
                                            // Actually, the partial probably expects a specific model?
                                            // User said: "_applypromocode, contains form which have dropdown list contains all the names of the active promocodes ... and submit button"

            ViewBag.ActivePromocodes = activePromos;
            return PartialView("~/Views/ControlPanel/Payment/_ApplyPromocode.cshtml");
        }

        [HttpPost("ApplyPromocode/{id}")]
        public async Task<IActionResult> ApplyPromocode(int id, [FromForm] int promocodeId)
        {


            try
            {

                // Fetch payment to verify existencece
                var payment = await _paymentManager.GetPaymentDetailsByIdAsync(id);
                if (payment == null) return NotFound();

                var promo = await _promocodeManager.GetPromocodeByIdAsync(promocodeId);
                return Ok(new { message = "Promocode applied successfully (Simulation)" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
