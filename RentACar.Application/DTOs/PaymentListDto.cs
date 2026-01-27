using System;

namespace RentACar.Application.DTOs
{
    public class PaymentListDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerUsername { get; set; }
        
        public string? CarModel { get; set; }
        public string? CarPlate { get; set; }
        
        public decimal Amount { get; set; }
        public DateOnly PaymentDate { get; set; }
        public string? PaymentMethodName { get; set; } // Renamed from PaymentMethod to match view
        public string? Status { get; set; }
        public string? PaymentProvider { get; set; }
        
        public string? BookingStatus { get; set; }
        public decimal? BookingSubtotal { get; set; }
        public decimal? BookingTotal { get; set; }
        
        public string? PromocodeName { get; set; }
        public decimal? PromocodeDiscountPercentage { get; set; }
    }
}
