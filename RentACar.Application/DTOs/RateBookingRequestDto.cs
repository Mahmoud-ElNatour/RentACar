using System;

namespace RentACar.Application.DTOs
{
    public class RateBookingRequestDto
    {
        public int BookingId { get; set; }
        public int Stars { get; set; }
        public string? Feedback { get; set; }
    }
}
