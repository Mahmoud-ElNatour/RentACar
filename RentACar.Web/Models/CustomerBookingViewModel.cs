using System;

namespace RentACar.Web.Models
{
    public class CustomerBookingViewModel
    {
        public int CarId { get; set; }
        public string CarModel { get; set; }
        public byte[]? CarImage { get; set; }
        public decimal PricePerDay { get; set; }
        public DateOnly? SuggestedStart { get; set; }
        public DateOnly? SuggestedEnd { get; set; }
    }
}
