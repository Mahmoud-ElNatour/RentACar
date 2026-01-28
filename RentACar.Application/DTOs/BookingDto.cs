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

        [StringLength(200)]
        public string? PickupLocationName { get; set; }

        [StringLength(200)]
        public string? PickupLocationLabel { get; set; }

        public DateTime? PickupDateTime { get; set; }

        // ✅ ADD THESE (for Google Maps & tracking)
        public double? PickupLatitude { get; set; }
        public double? PickupLongitude { get; set; }

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

        [StringLength(200)]
        public string? PickupLocationName { get; set; }

        [StringLength(200)]
        public string? PickupLocationLabel { get; set; }

        public DateTime? PickupDateTime { get; set; }

        // 🔹 OPTIONAL (future pin-on-map support)
        public double? PickupLatitude { get; set; }
        public double? PickupLongitude { get; set; }

        public string? Promocode { get; set; }
        public int PaymentMethodId { get; set; }
        public int? CreditcardId { get; set; }
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
        public decimal TotalPrice { get; set; }
        public string BookingStatus { get; set; } = "Pending";

        public bool HasDriver { get; set; }
        public int? DriverId { get; set; }

        // 🔹 Optional (admin edits / future use)
        public double? PickupLatitude { get; set; }
        public double? PickupLongitude { get; set; }
    }

    public class BookingCreationResultDto
    {
        public BookingDto Booking { get; set; } = new();
        public string? RedirectUrl { get; set; }
        public int? PaymentId { get; set; }
    }
}
