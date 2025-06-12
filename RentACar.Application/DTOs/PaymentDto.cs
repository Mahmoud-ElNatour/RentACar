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

        [StringLength(20)]
        public string? PaymentMethod { get; set; }

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
        [StringLength(20)]
        public string PaymentMethod { get; set; } // "Cash" or "CreditCard"

        public int? CreditcardId { get; set; } // Required if PaymentMethod is "CreditCard"
    }
}