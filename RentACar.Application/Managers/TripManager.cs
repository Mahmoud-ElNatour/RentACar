using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Application.Managers;

public class TripManager
{
    private readonly ITripRepository _tripRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;
    private readonly AuditLogManager _auditLogManager;

    private static readonly Dictionary<TripStatus, TripStatus[]> AllowedTransitions = new()
    {
        { TripStatus.Assigned, new[] { TripStatus.OnTheWay, TripStatus.Cancelled } },
        { TripStatus.OnTheWay, new[] { TripStatus.Arrived, TripStatus.Cancelled } },
        { TripStatus.Arrived, new[] { TripStatus.InTrip, TripStatus.Cancelled } },
        { TripStatus.InTrip, new[] { TripStatus.Completed, TripStatus.Cancelled } }
    };

    public TripManager(
        ITripRepository tripRepository,
        IBookingRepository bookingRepository,
        IMapper mapper,
        AuditLogManager auditLogManager)
    {
        _tripRepository = tripRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _auditLogManager = auditLogManager;
    }

    public async Task<List<TripDto>> GetTripsByDriverIdAsync(int driverId)
    {
        var trips = await _tripRepository.GetTripsByDriverIdAsync(driverId);
        return _mapper.Map<List<TripDto>>(trips);
    }

    public async Task<TripActionResult> StartTrackingAsync(int bookingId, int driverId)
    {
        return await ChangeStatusAsync(bookingId, driverId, TripStatus.OnTheWay, "Trip.StartTracking");
    }

    public async Task<TripActionResult> MarkArrivedAsync(int bookingId, int driverId)
    {
        return await ChangeStatusAsync(bookingId, driverId, TripStatus.Arrived, "Trip.Arrived");
    }

    public async Task<TripActionResult> StartTripAsync(int bookingId, int driverId)
    {
        return await ChangeStatusAsync(bookingId, driverId, TripStatus.InTrip, "Trip.Started");
    }

    public async Task<TripActionResult> CompleteTripAsync(int bookingId, int driverId)
    {
        return await ChangeStatusAsync(bookingId, driverId, TripStatus.Completed, "Trip.Completed");
    }

    public async Task<TripActionResult> CancelTripAsync(int bookingId, int driverId, string? reason)
    {
        var (trip, booking, errorResult) = await LoadTripAsync(bookingId, driverId, createIfMissing: true);
        if (errorResult != null)
        {
            return errorResult;
        }

        if (trip == null || booking == null)
        {
            return TripActionResult.Failure(TripActionError.NotFound, "Trip not found.");
        }

        if (IsTerminal(trip.TripStatus))
        {
            return TripActionResult.Failure(TripActionError.TerminalState, "Trip is already completed or cancelled.");
        }

        var now = DateTime.UtcNow;
        trip.TripStatus = TripStatus.Cancelled;
        trip.CancelledAt = now;
        trip.CancelReason = string.IsNullOrWhiteSpace(reason) ? trip.CancelReason : reason;
        trip.UpdatedAt = now;

        await _tripRepository.UpdateTripAsync(trip);
        await _auditLogManager.LogEventAsync("Trip.Cancelled", "Trip", trip.TripId.ToString(),
            $"Trip cancelled for booking {bookingId}", null, "Success");

        return TripActionResult.SuccessResult(_mapper.Map<TripDto>(trip), "Trip cancelled.");
    }

    public async Task<TripActionResult> UpdateDriverLocationAsync(int bookingId, int driverId, decimal latitude, decimal longitude, DateTime? timestamp)
    {
        var (trip, booking, errorResult) = await LoadTripAsync(bookingId, driverId, createIfMissing: true);
        if (errorResult != null)
        {
            return errorResult;
        }

        if (trip == null || booking == null)
        {
            return TripActionResult.Failure(TripActionError.NotFound, "Trip not found.");
        }

        if (IsTerminal(trip.TripStatus))
        {
            return TripActionResult.Failure(TripActionError.TerminalState, "Trip is already completed or cancelled.");
        }

        var now = timestamp == default ? DateTime.UtcNow : timestamp.Value.ToUniversalTime();
        trip.LastDriverLatitude = latitude;
        trip.LastDriverLongitude = longitude;
        trip.LastLocationUpdatedAt = now;
        trip.UpdatedAt = DateTime.UtcNow;

        await _tripRepository.UpdateTripAsync(trip);

        return TripActionResult.SuccessResult(_mapper.Map<TripDto>(trip), "Location updated.");
    }

    private async Task<TripActionResult> ChangeStatusAsync(int bookingId, int driverId, TripStatus nextStatus, string auditAction)
    {
        var (trip, booking, errorResult) = await LoadTripAsync(bookingId, driverId, createIfMissing: true);
        if (errorResult != null)
        {
            return errorResult;
        }

        if (trip == null || booking == null)
        {
            return TripActionResult.Failure(TripActionError.NotFound, "Trip not found.");
        }

        if (IsTerminal(trip.TripStatus))
        {
            return TripActionResult.Failure(TripActionError.TerminalState, "Trip is already completed or cancelled.");
        }

        if (trip.TripStatus != nextStatus && !IsValidTransition(trip.TripStatus, nextStatus))
        {
            return TripActionResult.Failure(TripActionError.InvalidTransition,
                $"Cannot change trip status from {trip.TripStatus} to {nextStatus}.");
        }

        var now = DateTime.UtcNow;
        if (trip.TripStatus != nextStatus)
        {
            trip.TripStatus = nextStatus;
        }

        switch (nextStatus)
        {
            case TripStatus.OnTheWay:
                trip.StartedAt ??= now;
                break;
            case TripStatus.Arrived:
                trip.ArrivedAt ??= now;
                break;
            case TripStatus.InTrip:
                trip.TripStartedAt ??= now;
                break;
            case TripStatus.Completed:
                trip.CompletedAt = now;
                break;
        }

        trip.UpdatedAt = now;

        await _tripRepository.UpdateTripAsync(trip);
        await _auditLogManager.LogEventAsync(auditAction, "Trip", trip.TripId.ToString(),
            $"Trip status changed to {trip.TripStatus} for booking {bookingId}", null, "Success");

        return TripActionResult.SuccessResult(_mapper.Map<TripDto>(trip), "Trip updated.");
    }

    private async Task<(Trip? trip, Booking? booking, TripActionResult? errorResult)> LoadTripAsync(int bookingId, int driverId, bool createIfMissing)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        if (booking == null)
        {
            return (null, null, TripActionResult.Failure(TripActionError.NotFound, "Booking not found."));
        }

        if (!booking.DriverId.HasValue || booking.DriverId.Value != driverId)
        {
            return (null, booking, TripActionResult.Failure(TripActionError.Forbidden, "You are not assigned to this booking."));
        }

        var trip = await _tripRepository.GetTripByBookingIdAsync(bookingId);
        if (trip == null && createIfMissing)
        {
            var now = DateTime.UtcNow;
            trip = new Trip
            {
                BookingId = bookingId,
                DriverId = booking.DriverId,
                TripStatus = TripStatus.Assigned,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _tripRepository.CreateTripAsync(trip);
        }

        if (trip != null && trip.DriverId == null)
        {
            trip.DriverId = booking.DriverId;
            trip.UpdatedAt = DateTime.UtcNow;
            await _tripRepository.UpdateTripAsync(trip);
        }

        return (trip, booking, null);
    }

    private static bool IsValidTransition(TripStatus current, TripStatus next)
    {
        return AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Contains(next);
    }

    private static bool IsTerminal(TripStatus status)
    {
        return status == TripStatus.Completed || status == TripStatus.Cancelled;
    }
}

public class TripProfile : Profile
{
    public TripProfile()
    {
        CreateMap<Trip, TripDto>()
            .ForMember(dest => dest.TripStatus, opt => opt.MapFrom(src => src.TripStatus.ToString()));
    }
}
