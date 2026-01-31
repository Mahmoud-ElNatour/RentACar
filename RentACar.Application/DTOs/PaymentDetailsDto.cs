using System;

namespace RentACar.Application.DTOs
{
    public class PaymentDetailsDto
    {
        public int PaymentId { get; set; }

        public int BookingId { get; set; }

        public decimal Amount { get; set; }

        public DateOnly PaymentDate { get; set; }

<<<<<<< HEAD
        public string? PaymentMethodName { get; set; }

=======
        public int? CreditcardId { get; set; }

        public string? PaymentMethodName { get; set; }

        public int? PaymentMethodId { get; set; }

>>>>>>> Mahmoud-V3
        public string? Status { get; set; }

        public string? PaymentProvider { get; set; }

        public string? PaymentProviderSessionId { get; set; }

<<<<<<< HEAD
=======
        public string? PaymentProviderPaymentIntentId { get; set; }

>>>>>>> Mahmoud-V3
        public string? CustomerName { get; set; }

        public string? CustomerUsername { get; set; }

        public string? BookingStatus { get; set; }

<<<<<<< HEAD
        public decimal? BookingSubtotal { get; set; }

=======
        public decimal? BookingTotal { get; set; }

        public decimal? BookingSubtotal { get; set; }

        public decimal? BookingDiscountAmount { get; set; }

>>>>>>> Mahmoud-V3
        public string? PromocodeName { get; set; }

        public decimal? PromocodeDiscountPercentage { get; set; }

        public string? CarModel { get; set; }

        public string? CarPlateNumber { get; set; }
<<<<<<< HEAD

        public DateOnly? BookingStartDate { get; set; }

        public DateOnly? BookingEndDate { get; set; }
=======
>>>>>>> Mahmoud-V3
    }
}
