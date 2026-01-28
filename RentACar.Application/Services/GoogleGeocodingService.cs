using Microsoft.Extensions.Configuration;
using RentACar.Application.Services;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class GoogleGeocodingService : IGoogleGeocodingService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public GoogleGeocodingService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["GOOGLE_MAPS_API_KEY"]
                  ?? throw new InvalidOperationException("GOOGLE_MAPS_API_KEY missing");
    }

    public async Task<(double lat, double lng)?> GeocodeAsync(string address)
    {
        var url =
            $"https://maps.googleapis.com/maps/api/geocode/json" +
            $"?address={Uri.EscapeDataString(address)}&key={_apiKey}";

        var response = await _http.GetStringAsync(url);
        using var json = JsonDocument.Parse(response);

        var status = json.RootElement.GetProperty("status").GetString();
        if (status != "OK") return null;

        var location = json.RootElement
            .GetProperty("results")[0]
            .GetProperty("geometry")
            .GetProperty("location");

        return (
            location.GetProperty("lat").GetDouble(),
            location.GetProperty("lng").GetDouble()
        );
    }
}
