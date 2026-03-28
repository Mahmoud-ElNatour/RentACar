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

            return PartialView("~/Views/ControlPanel/Payment/_EditPayment.cshtml", payment);
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

            return PartialView("~/Views/ControlPanel/Payment/_ViewPayment.cshtml", payment);
        }

        [HttpGet("~/Payment/Receipt/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Receipt(int id)
        {
            var payment = await _paymentManager.GetPaymentDetailsByIdAsync(id);
            if (payment == null) return NotFound();

            return View("~/Views/ControlPanel/Payment/Receipt.cshtml", payment);
        }

        [HttpGet]
        [HttpGet]
        public async Task<ActionResult<PaymentResultDto>> Get([FromQuery] PaymentFilterDto filter)
        {
            var result = await _paymentManager.GetPaymentsAsync(filter);
            return Ok(result);
        }

        [HttpGet("Stats")]
        public async Task<ActionResult<PaymentStatsDto>> GetStats([FromQuery] PaymentFilterDto filter)
        {
            var stats = await _paymentManager.GetPaymentStatsAsync(filter);
            return Ok(stats);
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

            return Ok();
        }

        [HttpGet("~/Payment/ApplyPromocode/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> ApplyPromocodeView(int id)
        {
            try
            {
                var payment = await _paymentManager.GetPaymentDetailsByIdAsync(id);
                if (payment == null) return NotFound();

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                IEnumerable<PromocodeDto> allPromos = new List<PromocodeDto>();
                try
                {
                    allPromos = await _promocodeManager.GetAllPromocodesAsync(userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load promocodes");
                    // Continue with empty list
                }

                var activePromos = allPromos.Where(p => p.IsActive).ToList();

                ViewBag.PaymentId = id;
                ViewBag.ActivePromocodes = activePromos;
                return PartialView("~/Views/ControlPanel/Payment/_ApplyPromocode.cshtml");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error in ApplyPromocodeView");
                 return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("ApplyPromocode/{id}")]
        public async Task<IActionResult> ApplyPromocode(int id, [FromForm] int promocodeId)
        {
            try
            {
                // Fetch payment to verify (Manager validation checks exists, but we can double check or just call)
                var success = await _paymentManager.ApplyPromocodeToPaymentAsync(id, promocodeId);
                
                if (success)
                {
                    return Ok(new { message = "Promocode applied successfully" });
                }
                else
                {
                     return BadRequest("Failed to apply promocode (Invalid code or payment not found).");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
