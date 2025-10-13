using System.Text.Json.Serialization;

namespace RentACar.Application.Integration.BookingCom;

public class BookingComHotelBookingRequest
{
    [JsonPropertyName("destination")]
    public string Destination { get; init; } = string.Empty;

    [JsonPropertyName("destination_country_code")]
    public string DestinationCountryCode { get; init; } = string.Empty;

    [JsonPropertyName("user_country_code")]
    public string UserCountryCode { get; init; } = string.Empty;

    [JsonPropertyName("checkin_date")]
    public string CheckInDate { get; init; } = string.Empty;

    [JsonPropertyName("checkout_date")]
    public string CheckOutDate { get; init; } = string.Empty;

    [JsonPropertyName("number_of_guests")]
    public int NumberOfGuests { get; init; }
        = 1;

    [JsonPropertyName("customer_reference")]
    public string CustomerReference { get; init; } = string.Empty;
}
