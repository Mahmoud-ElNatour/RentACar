using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RentACar.Application.Integration.BookingCom;

public class BookingComClient : IBookingComClient
{
    private readonly HttpClient _httpClient;
    private readonly BookingComOptions _options;
    private readonly ILogger<BookingComClient> _logger;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public BookingComClient(HttpClient httpClient, IOptions<BookingComOptions> options, ILogger<BookingComClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task<BookingComClientResponse> CreateHotelBookingAsync(BookingComHotelBookingRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync(request, _options.HotelBookingEndpoint, cancellationToken);
    }

    public Task<BookingComClientResponse> CreateFlightBookingAsync(BookingComFlightBookingRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync(request, _options.FlightBookingEndpoint, cancellationToken);
    }

    private async Task<BookingComClientResponse> SendAsync<TPayload>(TPayload payload, string endpoint, CancellationToken cancellationToken)
    {
        var requestJson = JsonSerializer.Serialize(payload, _serializerOptions);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _logger.LogInformation("Sending request to Booking.com endpoint {Endpoint}: {Payload}", endpoint, requestJson);

        using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation("Received response from Booking.com endpoint {Endpoint}: {Status} {Body}", endpoint, response.StatusCode, responseJson);

        return new BookingComClientResponse(
            response.IsSuccessStatusCode,
            response.StatusCode,
            responseJson,
            TryExtractReference(responseJson)
        );
    }

    private static string? TryExtractReference(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("reference", out var reference))
                {
                    return reference.GetString();
                }

                if (root.TryGetProperty("booking_reference", out var bookingReference))
                {
                    return bookingReference.GetString();
                }

                if (root.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Object)
                {
                    if (dataElement.TryGetProperty("reference", out var innerReference))
                    {
                        return innerReference.GetString();
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Ignore malformed JSON and return null
        }

        return null;
    }
}
