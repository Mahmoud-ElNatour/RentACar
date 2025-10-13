using System.Net;

namespace RentACar.Application.Integration.BookingCom;

public record BookingComClientResponse(
    bool IsSuccessStatusCode,
    HttpStatusCode StatusCode,
    string RawBody,
    string? ProviderReference
);
