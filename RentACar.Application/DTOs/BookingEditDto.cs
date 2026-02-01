namespace RentACar.Application.DTOs
{
    public class BookingEditDto
    {
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public int CarId { get; set; }
        public DateOnly BookingDate { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal TotalCost { get; set; }
        public string? BookingStatus { get; set; }
    }
}
