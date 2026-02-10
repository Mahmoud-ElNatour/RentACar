using System;

namespace RentACar.Application.DTOs
{
    public class BookingDetailsDto
    {
        public int BookingId { get; set; }
        public string? BookingStatus { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal? Subtotal { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerUsername { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? EmployeeName { get; set; }
        public string? CarModel { get; set; }
        public string? CarPlateNumber { get; set; }
        public string? CarCategory { get; set; }
        public string? CarColor { get; set; }
        public int? CarModelYear { get; set; }
        public decimal? CarPricePerDay { get; set; }
        public string? CarImageUrl { get; set; }
        public int? PaymentId { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string? PromocodeName { get; set; }
        public decimal? PromocodeDiscount { get; set; }
        public int? VerifiedRatingStars { get; set; }
        public bool IsRated { get; set; }
        public string? RatingFeedback { get; set; }
    }
}
