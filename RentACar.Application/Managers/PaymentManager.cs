using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Application.Services;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using AspNetUserEntity = RentACar.Core.Entities.AspNetUser;
using Microsoft.AspNetCore.Http;

namespace RentACar.Application.Managers
{
    public class PaymentManager
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Services.IStripePaymentService _stripePaymentService;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentManager> _logger;
        private readonly AuditLogManager _auditLogManager;

        public PaymentManager(
            IPaymentRepository paymentRepository,
            IBookingRepository bookingRepository,
            ICreditCardRepository creditCardRepository,
            IPaymentMethodRepository paymentMethodRepository,
            UserManager<IdentityUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            Services.IStripePaymentService stripePaymentService,
            IMapper mapper,
            ILogger<PaymentManager> logger,
            AuditLogManager auditLogManager)
        {
            _paymentRepository = paymentRepository;
            _bookingRepository = bookingRepository;
            _creditCardRepository = creditCardRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _stripePaymentService = stripePaymentService;
            _mapper = mapper;
            _logger = logger;
            _auditLogManager = auditLogManager;
        }

        public async Task<MakePaymentResultDto?> MakePaymentByCustomerAsync(MakePaymentRequestDto paymentDto, int customerUserId)
        {
            _logger.LogInformation("Customer {Id} making payment for booking {Booking}", customerUserId, paymentDto.BookingId);

            var booking = await _bookingRepository.GetByIdAsync(paymentDto.BookingId);
            if (booking == null || booking.CustomerId != customerUserId)
                return null;

            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentDto.PaymentMethodId);
            if (paymentMethod == null)
                return null;

            if (paymentMethod.PaymentMethodName.Equals("creditcard", StringComparison.OrdinalIgnoreCase))
            {
                var payment = new Payment
                {
                    BookingId = paymentDto.BookingId,
                    Amount = paymentDto.Amount,
                    PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    PaymentMethod = paymentMethod.PaymentMethodName,
                    Status = "pending",
                    PaymentProvider = "Stripe"
                };

                var created = await _paymentRepository.AddAsync(payment);
                var session = await CreateStripeCheckoutSessionAsync(created);

                if (string.IsNullOrWhiteSpace(session.CheckoutUrl))
                {
                    _logger.LogWarning("Stripe checkout session missing URL for payment {PaymentId}", created.PaymentId);
                    return null;
                }

                created.PaymentProviderSessionId = session.SessionId;
                created.PaymentProviderPaymentIntentId = session.PaymentIntentId;
                await _paymentRepository.UpdateAsync(created);

                var result = _mapper.Map<PaymentDto>(created);
                result.PaymentMethodId = paymentMethod.Id;
                return new MakePaymentResultDto
                {
                    Payment = result,
                    RequiresRedirect = true,
                    RedirectUrl = session.CheckoutUrl
                };
            }
            else
            {
                var payment = new Payment
                {
                    BookingId = paymentDto.BookingId,
                    Amount = paymentDto.Amount,
                    PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    PaymentMethod = paymentMethod.PaymentMethodName,
                    Status = "done"
                };
                await _paymentRepository.AddAsync(payment);
                await _auditLogManager.LogAsync("Create", "Payment", payment.PaymentId.ToString(), $"Customer payment of {payment.Amount:C} via {payment.PaymentMethod}");
                var dto = _mapper.Map<PaymentDto>(payment);
                dto.PaymentMethodId = paymentMethod.Id;
                return new MakePaymentResultDto
                {
                    Payment = dto,
                    RequiresRedirect = false
                };

            }
        }

        public async Task<MakePaymentResultDto?> MakePaymentByEmployeeAsync(MakePaymentRequestDto paymentDto, string employeeUserId)
        {
            _logger.LogInformation("Employee {Id} recording payment for booking {Booking}", employeeUserId, paymentDto.BookingId);
            var user = await _userManager.FindByIdAsync(employeeUserId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Employee"))
                return null;
            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentDto.PaymentMethodId);
            if (paymentMethod == null)
                return null;

            if (paymentMethod.PaymentMethodName.Equals("creditcard", StringComparison.OrdinalIgnoreCase))
            {
                if (!paymentDto.CreditcardId.HasValue)
                    return null;

                var creditCard = await _creditCardRepository.GetByIdAsync(paymentDto.CreditcardId.Value);
                if (creditCard == null)
                    return null;

                var payment = new Payment
                {
                    BookingId = paymentDto.BookingId,
                    Amount = paymentDto.Amount,
                    PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    CreditcardId = paymentDto.CreditcardId,
                    PaymentMethod = paymentMethod.PaymentMethodName,
                    Status = "done"
                };

                await _paymentRepository.AddAsync(payment);

                await _auditLogManager.LogAsync("Create", "Payment", payment.PaymentId.ToString(), $"Employee recorded payment of {payment.Amount:C}");

                var dto = _mapper.Map<PaymentDto>(payment);
                dto.PaymentMethodId = paymentMethod.Id;
                return new MakePaymentResultDto
                {
                    Payment = dto,
                    RequiresRedirect = false
                };
            }
            else
            {
                if (user == null || !await _userManager.IsInRoleAsync(user, "Employee"))
                    return null;

                var booking = await _bookingRepository.GetByIdAsync(paymentDto.BookingId);
                if (booking == null)
                    return null;

                if (paymentMethod == null || !paymentMethod.PaymentMethodName.Equals("cash", StringComparison.OrdinalIgnoreCase))
                    return null;

                var payment = new Payment
                {
                    BookingId = paymentDto.BookingId,
                    Amount = paymentDto.Amount,
                    PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    PaymentMethod = paymentMethod.PaymentMethodName,
                    Status = "done"
                };

                await _paymentRepository.AddAsync(payment);
                await _auditLogManager.LogAsync("Create", "Payment", payment.PaymentId.ToString(), $"Employee recorded cash payment: {payment.Amount:C}");
                var dto = _mapper.Map<PaymentDto>(payment);
                dto.PaymentMethodId = paymentMethod.Id;
                return new MakePaymentResultDto
                {
                    Payment = dto,
                    RequiresRedirect = false
                };
            }
        }

        public async Task<bool> MarkPaymentPaidAsync(int paymentId, string? paymentIntentId, string? sessionId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                _logger.LogWarning("Stripe webhook received for missing payment {PaymentId}", paymentId);
                return false;
            }

            payment.Status = "done";
            payment.PaymentProvider = "Stripe";
            if (!string.IsNullOrWhiteSpace(paymentIntentId))
            {
                payment.PaymentProviderPaymentIntentId = paymentIntentId;
            }

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                payment.PaymentProviderSessionId = sessionId;
            }

            await _paymentRepository.UpdateAsync(payment);
            return true;
        }

        public async Task<List<PaymentDto>> GetPaymentsByBookingIdAsync(int bookingId)
        {
            var payments = await _paymentRepository.GetPaymentsByBookingIdAsync(bookingId);
            return _mapper.Map<List<PaymentDto>>(payments);
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<List<PaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _paymentRepository.GetAllAsync();
            return _mapper.Map<List<PaymentDto>>(payments);
        }

        public async Task<List<PaymentDetailsDto>> GetAllPaymentsWithDetailsAsync()
        {
            var payments = await _paymentRepository.GetAllWithDetailsAsync();
            var lookup = await BuildPaymentMethodLookupAsync();
            return payments.Select(p => MapToDetailsDto(p, lookup)).ToList();
        }

        public async Task<PaymentDetailsDto?> GetPaymentDetailsByIdAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdWithDetailsAsync(id);
            if (payment == null)
                return null;

            var lookup = await BuildPaymentMethodLookupAsync();
            return MapToDetailsDto(payment, lookup);
        }

        public async Task<PaymentDto?> GetPaymentForEditAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
                return null;

            var dto = _mapper.Map<PaymentDto>(payment);
            var methodId = await ResolvePaymentMethodIdAsync(payment.PaymentMethod);
            if (methodId.HasValue)
            {
                dto.PaymentMethodId = methodId.Value;
            }
            else
            {
                var allMethods = await _paymentMethodRepository.GetAllAsync();
                var fallback = allMethods.FirstOrDefault();
                if (fallback != null)
                {
                    dto.PaymentMethodId = fallback.Id;
                }
            }
            return dto;
        }

        public async Task<PaymentDto?> AddPaymentAsync(PaymentDto dto)
        {
            var booking = await _bookingRepository.GetByIdAsync(dto.BookingId);
            if (booking == null)
            {
                _logger.LogWarning("Attempted to add payment for non-existing booking {BookingId}", dto.BookingId);
                return null;
            }

            var existingForBooking = await _paymentRepository.GetPaymentsByBookingIdAsync(dto.BookingId);
            if (existingForBooking.Any())
            {
                _logger.LogWarning("Booking {BookingId} already has an associated payment", dto.BookingId);
                return null;
            }

            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(dto.PaymentMethodId);
            if (paymentMethod == null)
            {
                _logger.LogWarning("Attempted to add payment with invalid method {MethodId}", dto.PaymentMethodId);
                return null;
            }

            if (paymentMethod.PaymentMethodName.Equals("creditcard", StringComparison.OrdinalIgnoreCase) && !dto.CreditcardId.HasValue)
            {
                _logger.LogWarning("Credit card payment requires a card id");
                return null;
            }

            if (dto.CreditcardId.HasValue)
            {
                var card = await _creditCardRepository.GetByIdAsync(dto.CreditcardId.Value);
                if (card == null)
                {
                    _logger.LogWarning("Invalid credit card {CardId} provided", dto.CreditcardId);
                    return null;
                }
            }

            var payment = new Payment
            {
                BookingId = dto.BookingId,
                Amount = dto.Amount,
                PaymentDate = dto.PaymentDate,
                CreditcardId = dto.CreditcardId,
                PaymentMethod = paymentMethod.PaymentMethodName,
                Status = dto.Status,
                PaymentProvider = dto.PaymentProvider,
                PaymentProviderSessionId = dto.PaymentProviderSessionId,
                PaymentProviderPaymentIntentId = dto.PaymentProviderPaymentIntentId
            };

            var created = await _paymentRepository.AddAsync(payment);
            if (booking != null)
            {
                await UpdateBookingStatusForPaymentAsync(booking, dto.Status);
            }
            var result = _mapper.Map<PaymentDto>(created);
            result.PaymentMethodId = paymentMethod.Id;
            return result;
        }

        public async Task<PaymentDto?> UpdatePaymentAsync(PaymentDto dto)
        {
            var existing = await _paymentRepository.GetByIdAsync(dto.PaymentId);
            if (existing == null)
            {
                _logger.LogWarning("Attempted to update missing payment {PaymentId}", dto.PaymentId);
                return null;
            }

            Booking? bookingForStatus = null;

            if (existing.BookingId != dto.BookingId)
            {
                var booking = await _bookingRepository.GetByIdAsync(dto.BookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Attempted to move payment {PaymentId} to non-existing booking {BookingId}", dto.PaymentId, dto.BookingId);
                    return null;
                }

                var otherPayments = await _paymentRepository.GetPaymentsByBookingIdAsync(dto.BookingId);
                if (otherPayments.Any(p => p.PaymentId != dto.PaymentId))
                {
                    _logger.LogWarning("Booking {BookingId} already has another payment", dto.BookingId);
                    return null;
                }

                existing.BookingId = dto.BookingId;
                bookingForStatus = booking;
            }

            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(dto.PaymentMethodId);
            if (paymentMethod == null)
            {
                _logger.LogWarning("Attempted to update payment {PaymentId} with invalid method {MethodId}", dto.PaymentId, dto.PaymentMethodId);
                return null;
            }

            if (paymentMethod.PaymentMethodName.Equals("creditcard", StringComparison.OrdinalIgnoreCase) && !dto.CreditcardId.HasValue)
            {
                _logger.LogWarning("Credit card payment update requires card id");
                return null;
            }

            if (dto.CreditcardId.HasValue)
            {
                var card = await _creditCardRepository.GetByIdAsync(dto.CreditcardId.Value);
                if (card == null)
                {
                    _logger.LogWarning("Invalid credit card {CardId} provided for update", dto.CreditcardId);
                    return null;
                }
            }

            existing.Amount = dto.Amount;
            existing.PaymentDate = dto.PaymentDate;
            existing.CreditcardId = dto.CreditcardId;
            existing.PaymentMethod = paymentMethod.PaymentMethodName;
            existing.Status = dto.Status;
            existing.PaymentProvider = dto.PaymentProvider;
            existing.PaymentProviderSessionId = dto.PaymentProviderSessionId;
            existing.PaymentProviderPaymentIntentId = dto.PaymentProviderPaymentIntentId;

            await _paymentRepository.UpdateAsync(existing);
            if (bookingForStatus == null)
            {
                bookingForStatus = await _bookingRepository.GetByIdAsync(existing.BookingId);
            }
            if (bookingForStatus != null)
            {
                await UpdateBookingStatusForPaymentAsync(bookingForStatus, dto.Status);
            }
            var result = _mapper.Map<PaymentDto>(existing);
            result.PaymentMethodId = paymentMethod.Id;
            return result;
        }

        public bool PrintPaymentDocument(int bookingId)
        {
            // To be implemented later
            return true;
        }

        private PaymentDetailsDto MapToDetailsDto(Payment payment, IReadOnlyDictionary<string, int>? methodLookup = null)
        {
            int? methodId = null;
            if (!string.IsNullOrWhiteSpace(payment.PaymentMethod) && methodLookup != null &&
                methodLookup.TryGetValue(payment.PaymentMethod, out var resolvedId))
            {
                methodId = resolvedId;
            }

            var subtotal = payment.Booking?.Subtotal;
            var promocode = payment.Booking?.Promocode;
            var discountPercentage = promocode?.DiscountPercentage;

            decimal? discountAmount = null;
            if (discountPercentage.HasValue && subtotal.HasValue)
            {
                discountAmount = Math.Round(subtotal.Value * discountPercentage.Value / 100m, 2, MidpointRounding.AwayFromZero);
            }

            var total = payment.Booking?.TotalPrice;
            if (!total.HasValue && subtotal.HasValue)
            {
                total = discountAmount.HasValue ? subtotal.Value - discountAmount.Value : subtotal.Value;
            }

            return new PaymentDetailsDto
            {
                PaymentId = payment.PaymentId,
                BookingId = payment.BookingId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                CreditcardId = payment.CreditcardId,
                PaymentMethodName = payment.PaymentMethod,
                Status = payment.Status,
                PaymentProvider = payment.PaymentProvider,
                PaymentProviderSessionId = payment.PaymentProviderSessionId,
                PaymentProviderPaymentIntentId = payment.PaymentProviderPaymentIntentId,
                CustomerName = payment.Booking?.Customer?.Name,
                CustomerUsername = payment.Booking?.Customer?.User?.UserName,
                BookingStatus = payment.Booking?.BookingStatus,
                BookingTotal = total,
                BookingSubtotal = subtotal,
                BookingDiscountAmount = discountAmount,
                PromocodeName = promocode?.Name,
                PromocodeDiscountPercentage = discountPercentage,
                CarModel = payment.Booking?.Car?.ModelName,
                CarPlateNumber = payment.Booking?.Car?.PlateNumber,
                PaymentMethodId = methodId
            };
        }

        private async Task<int?> ResolvePaymentMethodIdAsync(string? paymentMethodName)
        {
            if (string.IsNullOrWhiteSpace(paymentMethodName))
                return null;

            var methods = await _paymentMethodRepository.GetAllAsync();
            var match = methods.FirstOrDefault(m => m.PaymentMethodName.Equals(paymentMethodName, StringComparison.OrdinalIgnoreCase));
            return match?.Id;
        }

        private async Task<Dictionary<string, int>> BuildPaymentMethodLookupAsync()
        {
            var methods = await _paymentMethodRepository.GetAllAsync();
            return methods.ToDictionary(m => m.PaymentMethodName, m => m.Id, StringComparer.OrdinalIgnoreCase);
        }

        private async Task UpdateBookingStatusForPaymentAsync(Booking booking, string? paymentStatus)
        {
            if (string.IsNullOrWhiteSpace(paymentStatus))
            {
                return;
            }

            if (paymentStatus.Equals("rejected", StringComparison.OrdinalIgnoreCase) ||
                paymentStatus.Equals("cancelled", StringComparison.OrdinalIgnoreCase))
            {
                var targetStatus = paymentStatus.Equals("cancelled", StringComparison.OrdinalIgnoreCase)
                    ? "cancelled"
                    : "rejected";
                if (!string.Equals(booking.BookingStatus, targetStatus, StringComparison.OrdinalIgnoreCase))
                {
                    booking.BookingStatus = targetStatus;
                    await _bookingRepository.UpdateAsync(booking);
                }
            }
        }

        private async Task<StripeCheckoutSessionDto> CreateStripeCheckoutSessionAsync(Payment payment)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HTTP context is required to build Stripe return URLs.");
            }

            var domain = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
            var metadata = new Dictionary<string, string>
            {
                ["paymentId"] = payment.PaymentId.ToString(),
                ["bookingId"] = payment.BookingId.ToString()
            };

            var request = new StripeCheckoutSessionRequestDto
            {
                PaymentId = payment.PaymentId,
                BookingId = payment.BookingId,
                Amount = payment.Amount,
                Currency = "usd",
                SuccessUrl = $"{domain}/Stripe/Success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/Stripe/Cancel?paymentId={payment.PaymentId}",
                Description = $"Booking #{payment.BookingId}",
                Metadata = metadata
            };

            return await _stripePaymentService.CreateCheckoutSessionAsync(request);
        }



    }

    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<Payment, PaymentDto>().ReverseMap();
            CreateMap<PaymentMethod, PaymentMethodDto>().ReverseMap();
        }
    }
}
