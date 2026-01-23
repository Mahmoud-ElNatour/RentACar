using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs

{
    public class BookingDto
    {
        public int BookingId { get; set; }

        [Required]
        public int CustomerId { get; set; } = 0!;

        [Required]
        public int CarId { get; set; }

        public bool? IsBookedByEmployee { get; set; }

        public int? EmployeebookerId { get; set; }

        [Required]
        public DateOnly Startdate { get; set; }

        [Required]
        public DateOnly Enddate { get; set; }

        public int? PromocodeId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than 0.")]
        public decimal TotalPrice { get; set; }

        public bool HasDriver { get; set; }

        public int? DriverId { get; set; }

        public decimal? DriverDailyFee { get; set; }

        [StringLength(200)]
        public string? PickupAddress { get; set; }

        public DateTime? PickupDateTime { get; set; }

        [StringLength(50)]
        public string BookingStatus { get; set; } = "Pending";

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

        public bool HasDriver { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? DriverDailyFee { get; set; }

        [StringLength(200)]
        public string? PickupAddress { get; set; }

        public DateTime? PickupDateTime { get; set; }

        public string? Promocode { get; set; } // To apply promocode by string
        public int PaymentMethodId { get; set; } // "Cash" or "CreditCard"
        public int? CreditcardId { get; set; } // If paying by credit card
    }

    public class DeleteBookingRequestDto
    {
        [Required]
        public int BookingId { get; set; }
    }


    public class BookingEditDto
    {
        public int BookingId { get; set; }
        public DateOnly Startdate { get; set; }
        public DateOnly Enddate { get; set; }
        public decimal TotalPrice { get; set; } // still validated
        public string BookingStatus { get; set; } = "Pending";

        public bool HasDriver { get; set; }
        public int? DriverId { get; set; }
    }

    public class BookingCreationResultDto
    {
        public BookingDto Booking { get; set; } = new();
        public string? RedirectUrl { get; set; }
        public int? PaymentId { get; set; }
    }
}
