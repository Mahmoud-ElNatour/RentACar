using System.Threading;
using System.Threading.Tasks;

namespace RentACar.Application.Integration.BookingCom;

public interface IBookingComClient
{
    Task<BookingComClientResponse> CreateHotelBookingAsync(BookingComHotelBookingRequest request, CancellationToken cancellationToken = default);
    Task<BookingComClientResponse> CreateFlightBookingAsync(BookingComFlightBookingRequest request, CancellationToken cancellationToken = default);
}
