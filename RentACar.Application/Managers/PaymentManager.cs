using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using AspNetUserEntity = RentACar.Core.Entities.AspNetUser; // Add this using directive

namespace RentACar.Core.Managers
{
    public class PaymentManager
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly UserManager<AspNetUserEntity> _userManager; // Inject UserManager
        private readonly IMapper _mapper;

        public PaymentManager(
            IPaymentRepository paymentRepository,
            IBookingRepository bookingRepository,
            ICreditCardRepository creditCardRepository,
            UserManager<AspNetUserEntity> userManager, // Receive UserManager
            IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _bookingRepository = bookingRepository;
            _creditCardRepository = creditCardRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        // Used by authenticated customer (can only pay with credit card for their own booking)
        public async Task<bool> MakePaymentByCustomerAsync(MakePaymentRequestDto paymentDto, int customerUserId)
        {
            var booking = await _bookingRepository.GetByIdAsync(paymentDto.BookingId);
            if (booking == null || booking.CustomerId != customerUserId)
                return false;

            if (paymentDto.PaymentMethod?.ToLowerInvariant() != "creditcard" || !paymentDto.CreditcardId.HasValue)
                return false;

            var creditCard = await _creditCardRepository.GetByIdAsync(paymentDto.CreditcardId.Value);
            if (creditCard == null)
                return false;

            // Simulate payment gateway
            bool paymentSuccessful = true;

            var payment = new Payment
            {
                BookingId = paymentDto.BookingId,
                Amount = paymentDto.Amount,
                PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                CreditcardId = paymentDto.CreditcardId,
                PaymentMethod = paymentDto.PaymentMethod,
                Status = paymentSuccessful ? "done" : "failed"
            };

            await _paymentRepository.AddAsync(payment);
            return paymentSuccessful;
        }

        // Used by authenticated employee (can only pay in cash for any valid booking)
        public async Task<bool> MakePaymentByEmployeeAsync(MakePaymentRequestDto paymentDto, string employeeUserId)
        {
            var user = await _userManager.FindByIdAsync(employeeUserId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Employee")) // Check user role directly
                return false;

            var booking = await _bookingRepository.GetByIdAsync(paymentDto.BookingId);
            if (booking == null)
                return false;

            if (paymentDto.PaymentMethod?.ToLowerInvariant() != "cash")
                return false;

            var payment = new Payment
            {
                BookingId = paymentDto.BookingId,
                Amount = paymentDto.Amount,
                PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                PaymentMethod = paymentDto.PaymentMethod,
                Status = "done"
            };

            await _paymentRepository.AddAsync(payment);
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
        //add async task<bool> when implemented
        public bool PrintPaymentDocument(int bookingId)
        {
            // To be implemented later when other managers/entities are available
            return true;
        }
    }

    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<Payment, PaymentDto>().ReverseMap();
            // Note: MakePaymentRequestDto should be handled manually due to logic requirements.
        }
    }

}