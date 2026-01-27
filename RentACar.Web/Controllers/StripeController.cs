using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RentACar.Application.Managers;
using RentACar.Application.Services;

namespace RentACar.Web.Controllers
{
    [ApiController]
    [Route("Stripe")]
    public class StripeController : Controller
    {
        private readonly IStripePaymentService _stripePaymentService;
        private readonly PaymentManager _paymentManager;
        private readonly ILogger<StripeController> _logger;

        public StripeController(
            IStripePaymentService stripePaymentService,
            PaymentManager paymentManager,
            ILogger<StripeController> logger)
        {
            _stripePaymentService = stripePaymentService;
            _paymentManager = paymentManager;
            _logger = logger;
        }

        [HttpPost("Webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            string payload;
            using (var reader = new StreamReader(Request.Body))
            {
                payload = await reader.ReadToEndAsync();
            }

            var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
            var verification = _stripePaymentService.VerifyWebhookSignature(payload, signatureHeader);
            if (!verification.IsValid)
            {
                _logger.LogWarning("Stripe webhook signature invalid: {Error}", verification.ErrorMessage);
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(payload))
            {
                return BadRequest();
            }

            try
            {
                using var document = JsonDocument.Parse(payload);
                var root = document.RootElement;
                var eventType = root.GetProperty("type").GetString();
                if (string.Equals(eventType, "checkout.session.completed", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(eventType, "payment_intent.succeeded", StringComparison.OrdinalIgnoreCase))
                {
                    // Success Handled below
                }
                else if (string.Equals(eventType, "payment_intent.payment_failed", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(eventType, "checkout.session.async_payment_failed", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle Failure
                    var sessionObj = root.GetProperty("data").GetProperty("object");
                    var meta = sessionObj.TryGetProperty("metadata", out var m) ? m : default;
                    
                    if (meta.ValueKind != JsonValueKind.Undefined && 
                        meta.TryGetProperty("paymentId", out var pIdElem) && 
                        int.TryParse(pIdElem.GetString(), out var pId))
                    {
                        await _paymentManager.MarkPaymentFailedAsync(pId, "Stripe Webhook Failure");
                    }
                    return Ok();
                }
                else
                {
                    return Ok();
                }

                var session = root.GetProperty("data").GetProperty("object");
                var metadata = session.TryGetProperty("metadata", out var metadataElement)
                    ? metadataElement
                    : default;

                if (metadata.ValueKind == JsonValueKind.Undefined ||
                    !metadata.TryGetProperty("paymentId", out var paymentIdElement) ||
                    !int.TryParse(paymentIdElement.GetString(), out var paymentId))
                {
                    _logger.LogWarning("Stripe webhook missing paymentId metadata.");
                    return Ok();
                }

                var paymentIntentId = session.TryGetProperty("payment_intent", out var intentElement)
                    ? intentElement.GetString()
                    : null;
                var sessionId = session.TryGetProperty("id", out var sessionIdElement)
                    ? sessionIdElement.GetString()
                    : null;

                await _paymentManager.MarkPaymentPaidAsync(paymentId, paymentIntentId, sessionId);
                return Ok();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Stripe webhook payload invalid.");
                return BadRequest();
            }
        }

        [HttpGet("Success")]
        [AllowAnonymous]
        public IActionResult Success()
        {
            return View("~/Views/Stripe/Success.cshtml");
        }

        [HttpGet("Cancel")]
        [AllowAnonymous]
        public async Task<IActionResult> Cancel(int? paymentId = null)
        {
            if (paymentId.HasValue)
            {
                await _paymentManager.MarkPaymentCancelledAsync(paymentId.Value);
            }

            return View("~/Views/Stripe/Cancel.cshtml");
        }
    }
}
