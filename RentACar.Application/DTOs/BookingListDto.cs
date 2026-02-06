using System;

namespace RentACar.Application.DTOs
{
    public class BookingListDto
    {
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerUsername { get; set; }
        public string? CustomerEmail { get; set; }

        public int CarId { get; set; }
        public string? CarModel { get; set; }
        public string? CarPlate { get; set; }

        public int? EmployeebookerId { get; set; }
        public string? EmployeeName { get; set; }

        public int? PaymentId { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string? PaymentStatus { get; set; }

        public decimal? Subtotal { get; set; }
        public decimal TotalPrice { get; set; }

        public int? PromocodeId { get; set; }
        public string? PromocodeName { get; set; }
        public decimal? PromocodeDiscount { get; set; }

        public string Startdate { get; set; } = null!;
        public string Enddate { get; set; } = null!;

        public string BookingStatus { get; set; } = null!;
        public bool HasDriver { get; set; }
    }
}
