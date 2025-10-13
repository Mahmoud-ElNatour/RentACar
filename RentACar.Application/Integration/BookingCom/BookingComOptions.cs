namespace RentACar.Application.Integration.BookingCom;

public class BookingComOptions
{
    public string BaseUrl { get; set; } = "https://api.example.com/Booking.com/";
    public string HotelBookingEndpoint { get; set; } = "accommodations.bookings";
    public string FlightBookingEndpoint { get; set; } = "flights.bookings";
    public string? ApiKey { get; set; }
        = null;
}
