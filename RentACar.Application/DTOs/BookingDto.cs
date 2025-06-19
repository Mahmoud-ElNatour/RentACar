using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs

{
    public class BookingDto
    {
        public int BookingId { get; set; }

        [Required]
        [StringLength(450)]
        public int CustomerId { get; set; } = 0!;

        [Required]
        public int CarId { get; set; }

        public bool? IsBookedByEmployee { get; set; }

        [StringLength(450)]
        public int? EmployeebookerId { get; set; }

        [Required]
        public DateOnly Startdate { get; set; }

        [Required]
        public DateOnly Enddate { get; set; }

        public int? PromocodeId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than 0.")]
        public decimal TotalPrice { get; set; }

        [StringLength(50)]
        public string? BookingStatus { get; set; }

        [Required]
        public int PaymentId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Subtotal must be greater than 0.")]
        public decimal? Subtotal { get; set; }
    }

    public class MakeBookingRequestDto
    {
        [Required]
        public int CustomerId { get; set; } = 0!;

        [Required]
        public int CarId { get; set; }

        public bool IsBookedByEmployee { get; set; }

        public int EmployeebookerId { get; set; }

        [Required]
        public DateOnly Startdate { get; set; }

        [Required]
        public DateOnly Enddate { get; set; }

        public string? Promocode { get; set; } // To apply promocode by string
        public int PaymentMethodId { get; set; } // "Cash" or "CreditCard"
        public int? CreditcardId { get; set; } // If paying by credit card
    }

    public class DeleteBookingRequestDto
    {
        [Required]
        public int BookingId { get; set; }
    }
}