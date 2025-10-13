using System.Text.Json.Serialization;

namespace RentACar.Application.Integration.BookingCom;

public class BookingComFlightBookingRequest
{
    [JsonPropertyName("origin_airport_code")]
    public string OriginAirportCode { get; init; } = string.Empty;

    [JsonPropertyName("destination_airport_code")]
    public string DestinationAirportCode { get; init; } = string.Empty;

    [JsonPropertyName("departure_date")]
    public string DepartureDate { get; init; } = string.Empty;

    [JsonPropertyName("return_date")]
    public string? ReturnDate { get; init; }
        = null;

    [JsonPropertyName("adults")]
    public int Adults { get; init; } = 1;

    [JsonPropertyName("children")]
    public int Children { get; init; } = 0;

    [JsonPropertyName("cabin_class")]
    public string CabinClass { get; init; } = "ECONOMY";

    [JsonPropertyName("customer_reference")]
    public string CustomerReference { get; init; } = string.Empty;
}
