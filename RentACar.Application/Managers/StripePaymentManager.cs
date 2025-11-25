using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentACar.Application.DTOs;
using RentACar.Application.Options;
using RentACar.Core.Repositories;
using Stripe;

namespace RentACar.Application.Managers
{
    public class StripePaymentManager
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly StripeClient? _stripeClient;
        private readonly ILogger<StripePaymentManager> _logger;
        private readonly StripeOptions _options;

        public StripePaymentManager(
            IBookingRepository bookingRepository,
            IOptions<StripeOptions> stripeOptions,
            ILogger<StripePaymentManager> logger)
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
            _options = stripeOptions.Value;

            if (string.IsNullOrWhiteSpace(_options.SecretKey))
            {
                _logger.LogWarning("Stripe secret key is not configured. Stripe payments are disabled.");
                _stripeClient = null;
            }
            else
            {
                _stripeClient = new StripeClient(_options.SecretKey);
            }
        }

        public async Task<StripePaymentIntentResponseDto?> CreatePaymentIntentForBookingAsync(StripePaymentIntentRequestDto request)
        {
            if (_stripeClient == null)
            {
                _logger.LogWarning("Cannot create Stripe payment intent because the client is not configured.");
                return null;
            }

            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                _logger.LogWarning("Cannot create Stripe payment intent because booking {BookingId} was not found.", request.BookingId);
                return null;
            }

            if (booking.TotalPrice <= 0)
            {
                _logger.LogWarning("Cannot create Stripe payment intent because booking {BookingId} has non-positive total price {TotalPrice}.", request.BookingId, booking.TotalPrice);
                return null;
            }

            var normalizedCurrency = string.IsNullOrWhiteSpace(request.Currency)
                ? (_options.Currency ?? "usd")
                : request.Currency.ToLower(CultureInfo.InvariantCulture);

            var intentOptions = new PaymentIntentCreateOptions
            {
                Amount = ConvertToStripeAmount(booking.TotalPrice),
                Currency = normalizedCurrency,
                Description = $"Payment for booking {booking.BookingId}",
                ReceiptEmail = request.ReceiptEmail,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Metadata = new Dictionary<string, string>
                {
                    ["bookingId"] = booking.BookingId.ToString(CultureInfo.InvariantCulture),
                    ["carId"] = booking.CarId.ToString(CultureInfo.InvariantCulture),
                    ["customerId"] = booking.CustomerId.ToString(CultureInfo.InvariantCulture)
                }
            };

            try
            {
                var service = new PaymentIntentService(_stripeClient);
                var paymentIntent = await service.CreateAsync(intentOptions);

                _logger.LogInformation("Created Stripe payment intent {PaymentIntentId} for booking {BookingId}", paymentIntent.Id, booking.BookingId);

                return new StripePaymentIntentResponseDto
                {
                    PaymentIntentId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    Amount = paymentIntent.Amount ?? intentOptions.Amount.GetValueOrDefault(),
                    Currency = paymentIntent.Currency
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe rejected payment intent creation for booking {BookingId}: {Message}", booking.BookingId, ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating Stripe payment intent for booking {BookingId}", booking.BookingId);
                return null;
            }
        }

        private static long ConvertToStripeAmount(decimal amount)
        {
            return (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
        }
    }
}
