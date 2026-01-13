using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class StripePayoutSessionRequestDto
    {
        public int? PaymentId { get; set; }

        public int? BookingId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "usd";

        [StringLength(100)]
        public string? ConnectedAccountId { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }
    }

    public class StripePayoutSessionDto
    {
        public string SessionId { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = string.Empty;

        public string? ConnectedAccountId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? RawResponse { get; set; }
    }

    public class StripeWebhookVerificationResultDto
    {
        public bool IsValid { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
    }
}
