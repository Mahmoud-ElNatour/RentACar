using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        public DateOnly PaymentDate { get; set; }

        public int? CreditcardId { get; set; }

        // Use ID instead of raw string for better normalization
        [Required]
        public int PaymentMethodId { get; set; }
        public string? PaymentMethodName { get; set; }
        [StringLength(50)]
        public string? Status { get; set; }
    }

    public class MakePaymentRequestDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        public int PaymentMethodId { get; set; } // refers to PaymentMethods table

        public int? CreditcardId { get; set; } // Required if method is credit card
    }
}
