using System;

namespace RentACar.Application.DTOs
{
    public class CustomerRatingDisplayDto
    {
        public int RatingId { get; set; }
        public int BookingId { get; set; }
        public int Stars { get; set; }
        public string? Feedback { get; set; }
        public DateTime RatingDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
    }

    public class RatingDetailsDto : CustomerRatingDisplayDto
    {
        public DateTime BookingDate { get; set; }
        public string? EmployeeName { get; set; }
        public string? CustomerPhone { get; set; }
        public string CarName { get; set; } = string.Empty;
        public string CarPlateNumber { get; set; } = string.Empty;
        public string? CarImageBase64 { get; set; }
        public int? PaymentId { get; set; }
        public decimal? PaymentTotal { get; set; }
        public string? Promocode { get; set; }
    }

    public class EmployeeRatingSummaryDto
    {
        public int EmployeeId { get; set; }
        public double AverageStars { get; set; }
        public int TotalRatings { get; set; }
    }
}
