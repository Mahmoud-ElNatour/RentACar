using System;

namespace RentACar.Application.DTOs;

public class TripDto
{
    public int TripId { get; set; }
    public int BookingId { get; set; }
    public int? DriverId { get; set; }
    public string TripStatus { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? ArrivedAt { get; set; }
    public DateTime? TripStartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
    public decimal? LastDriverLatitude { get; set; }
    public decimal? LastDriverLongitude { get; set; }
    public DateTime? LastLocationUpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum TripActionError
{
    None,
    NotFound,
    Forbidden,
    InvalidTransition,
    TerminalState
}

public class TripActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public TripDto? Trip { get; set; }
    public TripActionError Error { get; set; }

    public static TripActionResult SuccessResult(TripDto trip, string message = "Success")
        => new() { Success = true, Message = message, Trip = trip, Error = TripActionError.None };

    public static TripActionResult Failure(TripActionError error, string message)
        => new() { Success = false, Message = message, Trip = null, Error = error };
}
