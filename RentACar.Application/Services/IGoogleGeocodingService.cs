using System.Threading.Tasks;

namespace RentACar.Application.Services
{
    public interface IGoogleGeocodingService
    {
        Task<(double lat, double lng)?> GeocodeAsync(string address);
    }
}
