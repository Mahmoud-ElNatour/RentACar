using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using AspNetUserEntity = RentACar.Core.Entities.AspNetUser;

namespace RentACar.Core.Managers
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

        public bool PrintPaymentDocument(int bookingId)
        {
            // To be implemented later
            return true;
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
