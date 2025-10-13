using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs;

public class HotelBookingRequestDto
{
    [Required]
    [MaxLength(120)]
    public string Destination { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string DestinationCountryCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string UserCountryCode { get; set; } = string.Empty;

    [Required]
    public DateOnly CheckInDate { get; set; }
        = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(30));

    [Required]
    public DateOnly CheckOutDate { get; set; }
        = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(33));

    [Range(1, 12)]
    public int NumberOfGuests { get; set; } = 1;

    [MaxLength(256)]
    public string? TargetCustomerUsername { get; set; }
        = null;
}

public class FlightBookingRequestDto
{
    [Required]
    [MaxLength(10)]
    public string OriginAirportCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string DestinationAirportCode { get; set; } = string.Empty;

    [Required]
    public DateOnly DepartureDate { get; set; }
        = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(45));

    public DateOnly? ReturnDate { get; set; }
        = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(52));

    [Range(1, 9)]
    public int Adults { get; set; } = 1;

    [Range(0, 8)]
    public int Children { get; set; } = 0;

    [MaxLength(20)]
    public string CabinClass { get; set; } = "ECONOMY";

    [MaxLength(256)]
    public string? TargetCustomerUsername { get; set; }
        = null;
}

public class TravelBookingResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ProviderReference { get; set; }
        = null;
    public string RawResponse { get; set; } = string.Empty;
    public int LoggedActionId { get; set; }
        = 0;
}

public class TravelActionLogDto
{
    public int TravelActionLogId { get; set; }
        = 0;
    public string CustomerUsername { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ActorUserName { get; set; } = string.Empty;
    public string ActorRole { get; set; } = string.Empty;
    public bool PerformedByEmployee { get; set; }
        = false;
    public bool IsSuccessful { get; set; }
        = false;
    public string? FailureReason { get; set; }
        = null;
    public string? ProviderReference { get; set; }
        = null;
    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;
}
