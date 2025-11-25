using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class StripePaymentIntentRequestDto
    {
        [Required]
        public int BookingId { get; set; }

        [StringLength(10, MinimumLength = 3)]
        public string? Currency { get; set; }

        [EmailAddress]
        public string? ReceiptEmail { get; set; }
    }

    public class StripePaymentIntentResponseDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;

        public string ClientSecret { get; set; } = string.Empty;

        public long Amount { get; set; }

        public string Currency { get; set; } = string.Empty;
    }
}
