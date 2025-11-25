using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;

namespace RentACar.Web.Controllers
{
    [ApiController]
    [Route("api/stripe")]
    [Authorize(Roles = "Admin,Employee,Customer")]
    public class StripePaymentController : ControllerBase
    {
        private readonly StripePaymentManager _stripePaymentManager;
        private readonly ILogger<StripePaymentController> _logger;

        public StripePaymentController(StripePaymentManager stripePaymentManager, ILogger<StripePaymentController> logger)
        {
            _stripePaymentManager = stripePaymentManager;
            _logger = logger;
        }

        [HttpPost("payment-intent")]
        public async Task<ActionResult<StripePaymentIntentResponseDto>> CreatePaymentIntent([FromBody] StripePaymentIntentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _stripePaymentManager.CreatePaymentIntentForBookingAsync(request);
            if (response == null)
            {
                _logger.LogWarning("Stripe payment intent could not be created for booking {BookingId}", request.BookingId);
                return BadRequest("Unable to create Stripe payment intent. Please verify booking details or server configuration.");
            }

            return Ok(response);
        }
    }
}
