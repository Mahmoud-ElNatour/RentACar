using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public int CarId { get; set; }
        public DateOnly Startdate { get; set; }
        public DateOnly Enddate { get; set; }
        public int? PromocodeId { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsBookedByEmployee { get; set; }
        public int? EmployeebookerId { get; set; }
        public string? CarModel { get; set; }
        public string? CarPlate { get; set; }
        
        [StringLength(50)]
        public string BookingStatus { get; set; } = "Pending";

        [Range(0.01, double.MaxValue, ErrorMessage = "Subtotal must be greater than 0.")]
        public decimal? Subtotal { get; set; }
        
        // Driver features
        public bool HasDriver { get; set; }
        public int? DriverId { get; set; }
        public decimal? DriverDailyFee { get; set; }
        
        public string? PickupAddress { get; set; }
        public string? PickupLocationLabel { get; set; }
        public DateTime? PickupDateTime { get; set; }
        public double? PickupLatitude { get; set; }
        public double? PickupLongitude { get; set; }
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
        public string? PaymentMethod { get; set; } // For string input (Card/Cash)
        // Driver features
        public bool HasDriver { get; set; }
        public decimal? DriverDailyFee { get; set; }
        public string? PickupAddress { get; set; }
        public string? PickupLocationName { get; set; }
        public DateTime? PickupDateTime { get; set; }
        public double? PickupLatitude { get; set; }
        public double? PickupLongitude { get; set; }
    }

    public class DeleteBookingRequestDto
    {
        [Required]
        public int BookingId { get; set; }
    }

    public class BookingCreationResultDto
    {
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public BookingDto Booking { get; set; } = new();
        public string? RedirectUrl { get; set; }
        public int? PaymentId { get; set; }
    }
}
