using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class StripeCheckoutSessionRequestDto
    {
        [Required]
        public int PaymentId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "usd";

        [Required]
        public string SuccessUrl { get; set; } = null!;

        [Required]
        public string CancelUrl { get; set; } = null!;

        public string? Description { get; set; }

        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class StripeCheckoutSessionDto
    {
        public string SessionId { get; set; } = string.Empty;

        public string CheckoutUrl { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? PaymentIntentId { get; set; }

        public string? RawResponse { get; set; }
    }

    public class StripeWebhookVerificationResultDto
    {
        public bool IsValid { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
    }
}
