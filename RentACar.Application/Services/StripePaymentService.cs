using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;

namespace RentACar.Application.Services
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StripePaymentService> _logger;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public StripePaymentService(
            HttpClient httpClient, 
            ILogger<StripePaymentService> logger,
            Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<StripeCheckoutSessionDto> CreateCheckoutSessionAsync(
            StripeCheckoutSessionRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var secretKey = RequireSecretKey();
            var amountInCents = ConvertToStripeAmount(request.Amount, request.Currency);

            using var message = new HttpRequestMessage(HttpMethod.Post, "v1/checkout/sessions");
            message.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                secretKey);

            var fields = new Dictionary<string, string>
            {
                ["mode"] = "payment",
                ["success_url"] = request.SuccessUrl,
                ["cancel_url"] = request.CancelUrl,
                ["line_items[0][quantity]"] = "1",
                ["line_items[0][price_data][currency]"] = request.Currency.ToLowerInvariant(),
                ["line_items[0][price_data][unit_amount]"] = amountInCents.ToString(CultureInfo.InvariantCulture),
                ["line_items[0][price_data][product_data][name]"] = string.IsNullOrWhiteSpace(request.Description)
                    ? "Rent a Car Payment"
                    : request.Description
            };

            if (request.Metadata != null)
            {
                foreach (var kv in request.Metadata)
                {
                    fields[$"metadata[{kv.Key}]"] = kv.Value;
                }
            }

            message.Content = new FormUrlEncodedContent(fields);

            using var response = await _httpClient.SendAsync(message, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Stripe checkout session creation failed with status {StatusCode}: {Response}",
                    response.StatusCode,
                    responseContent);
            }

            return ParseCheckoutSession(responseContent);
        }

        public StripeWebhookVerificationResultDto VerifyWebhookSignature(
            string payload,
            string signatureHeader,
            int toleranceSeconds = 300)
        {
            if (string.IsNullOrWhiteSpace(signatureHeader))
            {
                return new StripeWebhookVerificationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "Missing Stripe-Signature header."
                };
            }

            var webhookSecret = RequireWebhookSecret();
            var parsed = ParseSignatureHeader(signatureHeader);
            if (!parsed.Timestamp.HasValue || parsed.Signatures.Count == 0)
            {
                return new StripeWebhookVerificationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "Invalid Stripe-Signature header."
                };
            }

            var timestamp = parsed.Timestamp.Value;
            var expected = ComputeSignature(webhookSecret, timestamp, payload);
            var isValid = parsed.Signatures.Exists(sig => SecureEquals(sig, expected));

            if (!isValid)
            {
                return new StripeWebhookVerificationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "Signature verification failed.",
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp)
                };
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (Math.Abs(now - timestamp) > toleranceSeconds)
            {
                return new StripeWebhookVerificationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "Signature timestamp outside tolerance.",
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp)
                };
            }

            return new StripeWebhookVerificationResultDto
            {
                IsValid = true,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp)
            };
        }

        private string RequireSecretKey()
        {
            // Try Configuration (UserSecrets, AppSettings, EnvVars via Config Provider)
            var key = _configuration["Stripe:SecretKey"] ?? _configuration["STRIPE_PRIVATE_KEY"];
            
            if (string.IsNullOrWhiteSpace(key))
            {
                // Fallback to direct Environment Variable (just in case provider missed it)
                key = Environment.GetEnvironmentVariable("STRIPE_PRIVATE_KEY");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogError("Stripe Secret Key is MISSING. Checked 'Stripe:SecretKey', 'STRIPE_PRIVATE_KEY' in Config, and 'STRIPE_PRIVATE_KEY' in Env.");
                throw new InvalidOperationException(
                    "Stripe secret key missing. Please set 'Stripe:SecretKey' in appsettings.json or 'STRIPE_PRIVATE_KEY' environment variable.");
            }

            return key;
        }

        private string RequireWebhookSecret()
        {
             // Try Configuration (UserSecrets, AppSettings, EnvVars via Config Provider)
            var key = _configuration["Stripe:WebhookSecret"] ?? _configuration["STRIPE_WEBHOOK_SECRET"];

            if (string.IsNullOrWhiteSpace(key))
            {
                 // Fallback to direct Environment Variable
                key = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogError("Stripe Webhook Secret is MISSING. Checked 'Stripe:WebhookSecret', 'STRIPE_WEBHOOK_SECRET' in Config, and 'STRIPE_WEBHOOK_SECRET' in Env.");
                throw new InvalidOperationException(
                    "Stripe webhook secret missing. Please set 'Stripe:WebhookSecret' in appsettings.json or 'STRIPE_WEBHOOK_SECRET' environment variable.");
            }

            return key;
        }

        private static long ConvertToStripeAmount(decimal amount, string currency)
        {
            var zeroDecimalCurrencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga",
                "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf"
            };

            if (zeroDecimalCurrencies.Contains(currency))
            {
                return (long)Math.Round(amount, 0, MidpointRounding.AwayFromZero);
            }

            return (long)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
        }

        private static StripeCheckoutSessionDto ParseCheckoutSession(string response)
        {
            try
            {
                using var document = JsonDocument.Parse(response);
                var root = document.RootElement;

                return new StripeCheckoutSessionDto
                {
                    SessionId = root.TryGetProperty("id", out var idElement) ? idElement.GetString() ?? string.Empty : string.Empty,
                    CheckoutUrl = root.TryGetProperty("url", out var urlElement) ? urlElement.GetString() ?? string.Empty : string.Empty,
                    Status = root.TryGetProperty("status", out var statusElement)
                        ? statusElement.GetString() ?? string.Empty
                        : string.Empty,
                    PaymentIntentId = root.TryGetProperty("payment_intent", out var intentElement)
                        ? intentElement.GetString()
                        : null,
                    RawResponse = response
                };
            }
            catch (JsonException)
            {
                return new StripeCheckoutSessionDto { RawResponse = response };
            }
        }

        private static StripeSignatureHeader ParseSignatureHeader(string signatureHeader)
        {
            var result = new StripeSignatureHeader();
            var parts = signatureHeader.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var kv = part.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
                if (kv.Length != 2)
                {
                    continue;
                }

                if (kv[0] == "t" && long.TryParse(kv[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var timestamp))
                {
                    result.Timestamp = timestamp;
                    continue;
                }

                if (kv[0] == "v1")
                {
                    result.Signatures.Add(kv[1]);
                }
            }

            return result;
        }

        private static string ComputeSignature(string secret, long timestamp, string payload)
        {
            var signedPayload = $"{timestamp}.{payload}";
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(signedPayload);
            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(payloadBytes);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static bool SecureEquals(string left, string right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            var result = 0;
            for (var i = 0; i < left.Length; i++)
            {
                result |= left[i] ^ right[i];
            }

            return result == 0;
        }

        private sealed class StripeSignatureHeader
        {
            public long? Timestamp { get; set; }

            public List<string> Signatures { get; } = new();
        }

        public async Task<StripeCheckoutSessionDto> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            var secretKey = RequireSecretKey();
            using var message = new HttpRequestMessage(HttpMethod.Get, $"v1/checkout/sessions/{sessionId}");
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

            using var response = await _httpClient.SendAsync(message, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Stripe get session failed for {SessionId}: {Status} {Response}", sessionId, response.StatusCode, responseContent);
                return new StripeCheckoutSessionDto { RawResponse = responseContent };
            }

            return ParseCheckoutSession(responseContent);
        }
    }
}
