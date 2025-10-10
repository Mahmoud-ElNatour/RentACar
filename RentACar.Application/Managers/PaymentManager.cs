using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using AspNetUserEntity = RentACar.Core.Entities.AspNetUser;

namespace RentACar.Application.Managers
{
    public class PaymentManager
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentManager> _logger;

        public PaymentManager(
            IPaymentRepository paymentRepository,
            IBookingRepository bookingRepository,
            ICreditCardRepository creditCardRepository,
            IPaymentMethodRepository paymentMethodRepository,
            UserManager<IdentityUser> userManager,
            IMapper mapper,
            ILogger<PaymentManager> logger)
        {
            _paymentRepository = paymentRepository;
            _bookingRepository = bookingRepository;
            _creditCardRepository = creditCardRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> MakePaymentByCustomerAsync(MakePaymentRequestDto paymentDto, int customerUserId)
        {
            _logger.LogInformation("Customer {Id} making payment for booking {Booking}", customerUserId, paymentDto.BookingId);

            var booking = await _bookingRepository.GetByIdAsync(paymentDto.BookingId);
            if (booking == null || booking.CustomerId != customerUserId)
                return false;

            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentDto.PaymentMethodId);
            if (paymentMethod == null)
                return false;
            if (paymentMethod.PaymentMethodName.Equals("creditcard", StringComparison.OrdinalIgnoreCase))
            {
                if (!paymentDto.CreditcardId.HasValue)
                    return false;

                var creditCard = await _creditCardRepository.GetByIdAsync(paymentDto.CreditcardId.Value);
                if (creditCard == null)
                    return false;

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
                return true;
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
                return true;

            }
        }

        public async Task<bool> MakePaymentByEmployeeAsync(MakePaymentRequestDto paymentDto, string employeeUserId)
        {
            _logger.LogInformation("Employee {Id} recording payment for booking {Booking}", employeeUserId, paymentDto.BookingId);
            var user = await _userManager.FindByIdAsync(employeeUserId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Employee"))
                return false;
            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentDto.PaymentMethodId);
            if (paymentMethod == null)
                return false;

            if (paymentMethod.PaymentMethodName.Equals("creditcard", StringComparison.OrdinalIgnoreCase))
            {
                if (!paymentDto.CreditcardId.HasValue)
                    return false;

                var creditCard = await _creditCardRepository.GetByIdAsync(paymentDto.CreditcardId.Value);
                if (creditCard == null)
                    return false;

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
                return true;
            }
            else
            {
                if (user == null || !await _userManager.IsInRoleAsync(user, "Employee"))
                    return false;

                var booking = await _bookingRepository.GetByIdAsync(paymentDto.BookingId);
                if (booking == null)
                    return false;

                if (paymentMethod == null || !paymentMethod.PaymentMethodName.Equals("cash", StringComparison.OrdinalIgnoreCase))
                    return false;

                var payment = new Payment
                {
                    BookingId = paymentDto.BookingId,
                    Amount = paymentDto.Amount,
                    PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    PaymentMethod = paymentMethod.PaymentMethodName,
                    Status = "done"
                };

                await _paymentRepository.AddAsync(payment);
                return true;
            }
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
            //dto.PaymentMethodId = await ResolvePaymentMethodIdAsync(payment.PaymentMethod);
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
                Status = dto.Status
            };

            var created = await _paymentRepository.AddAsync(payment);
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

            await _paymentRepository.UpdateAsync(existing);
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
            if (!string.IsNullOrWhiteSpace(payment.PaymentMethod) && methodLookup != null)
            {
                if (methodLookup.TryGetValue(payment.PaymentMethod, out var id))
                {
                    methodId = id;
                }
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
                CustomerName = payment.Booking?.Customer?.Name,
                CustomerUsername = payment.Booking?.Customer?.User?.UserName,
                BookingStatus = payment.Booking?.BookingStatus,
                BookingTotal = payment.Booking?.TotalPrice,
                BookingSubtotal = payment.Booking?.Subtotal,
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
