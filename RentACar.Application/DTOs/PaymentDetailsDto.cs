using System;

namespace RentACar.Application.DTOs
{
    public class PaymentDetailsDto
    {
        public int PaymentId { get; set; }

        public int BookingId { get; set; }

        public decimal Amount { get; set; }

        public DateOnly PaymentDate { get; set; }
        public string? PaymentMethodName { get; set; }        public string? Status { get; set; }

        public string? PaymentProvider { get; set; }

        public string? PaymentProviderSessionId { get; set; }
        public string? PaymentProviderPaymentIntentId { get; set; }
        
        public string? CustomerName { get; set; }

        public string? CustomerUsername { get; set; }

        public string? BookingStatus { get; set; }

        public decimal? BookingSubtotal { get; set; }
        public string? PromocodeName { get; set; }

        public decimal? PromocodeDiscountPercentage { get; set; }

        public string? CarModel { get; set; }

        public string? CarPlateNumber { get; set; }

        public DateOnly? BookingStartDate { get; set; }

        public DateOnly? BookingEndDate { get; set; }
    }
}
