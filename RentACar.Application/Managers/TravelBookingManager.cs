using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Application.Integration.BookingCom;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Application.Managers;

public class TravelBookingManager
{
    private readonly IBookingComClient _bookingComClient;
    private readonly ITravelActionLogRepository _travelActionLogRepository;
    private readonly CustomerManager _customerManager;
    private readonly ILogger<TravelBookingManager> _logger;
    private readonly IMapper _mapper;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public TravelBookingManager(
        IBookingComClient bookingComClient,
        ITravelActionLogRepository travelActionLogRepository,
        CustomerManager customerManager,
        ILogger<TravelBookingManager> logger,
        IMapper mapper)
    {
        _bookingComClient = bookingComClient;
        _travelActionLogRepository = travelActionLogRepository;
        _customerManager = customerManager;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<TravelBookingResponseDto> BookHotelAsync(HotelBookingRequestDto request, IdentityUser actor, IList<string> actorRoles, CancellationToken cancellationToken = default)
    {
        if (request.CheckOutDate <= request.CheckInDate)
        {
            throw new InvalidOperationException("Checkout date must be after check-in date.");
        }

        var (customer, _) = await ResolveTargetCustomerAsync(request.TargetCustomerUsername, actor.UserName);
        var payload = new BookingComHotelBookingRequest
        {
            Destination = request.Destination,
            DestinationCountryCode = request.DestinationCountryCode,
            UserCountryCode = request.UserCountryCode,
            CheckInDate = request.CheckInDate.ToString("yyyy-MM-dd"),
            CheckOutDate = request.CheckOutDate.ToString("yyyy-MM-dd"),
            NumberOfGuests = request.NumberOfGuests,
            CustomerReference = customer.UserId.ToString()
        };

        var response = await _bookingComClient.CreateHotelBookingAsync(payload, cancellationToken);

        var log = await SaveLogAsync(
            actionType: "HotelBooking",
            actor,
            actorRoles,
            customer,
            payload,
            response,
            response.IsSuccessStatusCode ? null : BuildFailureMessage(response));

        return new TravelBookingResponseDto
        {
            Success = response.IsSuccessStatusCode,
            Message = response.IsSuccessStatusCode
                ? "Hotel booking request submitted successfully."
                : BuildFailureMessage(response),
            ProviderReference = response.ProviderReference,
            RawResponse = response.RawBody,
            LoggedActionId = log.TravelActionLogId
        };
    }

    public async Task<TravelBookingResponseDto> BookFlightAsync(FlightBookingRequestDto request, IdentityUser actor, IList<string> actorRoles, CancellationToken cancellationToken = default)
    {
        var (customer, _) = await ResolveTargetCustomerAsync(request.TargetCustomerUsername, actor.UserName);

        var payload = new BookingComFlightBookingRequest
        {
            OriginAirportCode = request.OriginAirportCode,
            DestinationAirportCode = request.DestinationAirportCode,
            DepartureDate = request.DepartureDate.ToString("yyyy-MM-dd"),
            ReturnDate = request.ReturnDate?.ToString("yyyy-MM-dd"),
            Adults = request.Adults,
            Children = request.Children,
            CabinClass = request.CabinClass,
            CustomerReference = customer.UserId.ToString()
        };

        var response = await _bookingComClient.CreateFlightBookingAsync(payload, cancellationToken);

        var log = await SaveLogAsync(
            actionType: "FlightBooking",
            actor,
            actorRoles,
            customer,
            payload,
            response,
            response.IsSuccessStatusCode ? null : BuildFailureMessage(response));

        return new TravelBookingResponseDto
        {
            Success = response.IsSuccessStatusCode,
            Message = response.IsSuccessStatusCode
                ? "Flight booking request submitted successfully."
                : BuildFailureMessage(response),
            ProviderReference = response.ProviderReference,
            RawResponse = response.RawBody,
            LoggedActionId = log.TravelActionLogId
        };
    }

    public async Task<List<TravelActionLogDto>> GetLogsAsync(string? customerUsername, int limit, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        List<TravelActionLog> logs;
        if (!string.IsNullOrWhiteSpace(customerUsername))
        {
            logs = await _travelActionLogRepository.GetByCustomerUsernameAsync(customerUsername, limit);
        }
        else if (fromUtc.HasValue && toUtc.HasValue)
        {
            logs = await _travelActionLogRepository.GetByDateRangeAsync(fromUtc.Value, toUtc.Value, limit);
        }
        else
        {
            logs = await _travelActionLogRepository.GetRecentAsync(limit);
        }

        return _mapper.Map<List<TravelActionLogDto>>(logs);
    }

    private async Task<(CustomerDTO customer, string username)> ResolveTargetCustomerAsync(string? targetUsername, string? fallbackUsername)
    {
        var username = !string.IsNullOrWhiteSpace(targetUsername) ? targetUsername.Trim() : fallbackUsername;
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException("No target customer username supplied.");
        }

        var customer = await _customerManager.GetCustomerByUsername(username);
        if (customer == null)
        {
            _logger.LogWarning("Customer {Username} not found when preparing travel booking.", username);
            throw new InvalidOperationException($"Customer '{username}' was not found.");
        }

        if (!customer.Isactive)
        {
            throw new InvalidOperationException("The selected customer is marked as inactive.");
        }

        return (customer, username);
    }

    private async Task<TravelActionLog> SaveLogAsync(
        string actionType,
        IdentityUser actor,
        IList<string> actorRoles,
        CustomerDTO customer,
        object payload,
        BookingComClientResponse response,
        string? failureReason)
    {
        var performedByEmployee = actorRoles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(r, "Employee", StringComparison.OrdinalIgnoreCase));

        var actorRole = actorRoles.FirstOrDefault() ?? "Customer";

        var log = new TravelActionLog
        {
            CustomerId = customer.UserId,
            CustomerUsername = customer.username,
            ActionType = actionType,
            Provider = "Booking.com",
            ActorAspNetUserId = actor.Id,
            ActorUserName = actor.UserName ?? actor.Email ?? string.Empty,
            ActorRole = actorRole,
            PerformedByEmployee = performedByEmployee,
            RequestPayload = JsonSerializer.Serialize(payload, _serializerOptions),
            ResponsePayload = response.RawBody,
            ProviderReference = response.ProviderReference,
            IsSuccessful = response.IsSuccessStatusCode,
            FailureReason = failureReason,
            CreatedAtUtc = DateTime.UtcNow
        };

        return await _travelActionLogRepository.AddAsync(log);
    }

    private static string BuildFailureMessage(BookingComClientResponse response)
    {
        return $"Booking.com rejected the request ({(int)response.StatusCode} {response.StatusCode}).";
    }
}
