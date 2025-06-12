using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

using AspNetUserEntity = RentACar.Application.DTOs.AspNetUser; // Add this using directive

namespace RentACar.Core.Managers
{
    public class BookingManager
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICarRepository _carRepository;
        private readonly IPromocodeRepository _promocodeRepository;
        private readonly PaymentManager _paymentManager;
        private readonly IMapper _mapper;

        public BookingManager(
            IBookingRepository bookingRepository,
            ICustomerRepository customerRepository,
            ICarRepository carRepository,
            IPromocodeRepository promocodeRepository,
            PaymentManager paymentManager,
            IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _customerRepository = customerRepository;
            _carRepository = carRepository;
            _promocodeRepository = promocodeRepository;
            _paymentManager = paymentManager;
            _mapper = mapper;
        }

        public async Task<List<BookingDto>> GetBookingHistoryAsync(int customerId)
        {
            var bookings = await _bookingRepository.GetBookingsByCustomerIdAsync(customerId);
            return _mapper.Map<List<BookingDto>>(bookings);
        }

        public async Task<List<BookingDto>> GetBookingsByEmployeeIdAsync(int employeeId)
        {
            var bookings = await _bookingRepository.GetBookingsByEmployeeIdAsync(employeeId);
            return _mapper.Map<List<BookingDto>>(bookings);
        }

        public async Task<BookingDto?> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            return _mapper.Map<BookingDto>(booking);
        }

        public async Task<BookingDto?> MakeBookingAsync(MakeBookingRequestDto requestDto, string loggedInUserId)
        {
            var customer = await _customerRepository.GetByIdAsync(requestDto.CustomerId);
            if (customer == null || !customer.IsVerified || !customer.Isactive)
                return null; // Or throw specific exceptions

            var car = await _carRepository.GetByIdAsync(requestDto.CarId);
            if (car == null || !car.IsAvailable)
                return null; // Or throw specific exceptions

            Promocode? promocode = null;
            if (!string.IsNullOrEmpty(requestDto.Promocode))
            {
                promocode = await _promocodeRepository.GetByCodeAsync(requestDto.Promocode);
                if (promocode == null || !promocode.IsActive || promocode.ValidUntil < DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    promocode = null; // Promocode invalid or not found
                }
            }

            decimal totalPrice = CalculateTotalPrice(requestDto.CarId, requestDto.Startdate, requestDto.Enddate);
            decimal subtotal = totalPrice;

            if (promocode != null)
            {
                totalPrice = ApplyPromocode(totalPrice, promocode);
            }

            if (requestDto.PaymentMethod?.ToLowerInvariant() == "creditcard")
            {
                var paymentResult = await _paymentManager.MakePaymentByCustomerAsync(
                    new MakePaymentRequestDto
                    {
                        BookingId = 0, // BookingId will be set after booking is created
                        Amount = totalPrice,
                        PaymentMethod = requestDto.PaymentMethod,
                        CreditcardId = requestDto.CreditcardId
                    },
                    requestDto.CustomerId);

                if (!paymentResult)
                    return null; // Payment failed for credit card
                // Payment ID will be set when the Payment entity is created in PaymentManager
            }
            else if (requestDto.PaymentMethod?.ToLowerInvariant() == "cash")
            {
                    var paymentResult = await _paymentManager.MakePaymentByEmployeeAsync(
                    new MakePaymentRequestDto
                    {
                        BookingId = 0, // BookingId will be set after booking is created
                        Amount = totalPrice,
                        PaymentMethod = requestDto.PaymentMethod
                    },
                    loggedInUserId);
                if (!paymentResult)
                    return null; // Payment failed for cash (unlikely in this simplified scenario)
                // Payment ID will be set when the Payment entity is created in PaymentManager
            }
            else
            {
                return null; // Invalid payment method
            }

            var bookingEntity = new Booking
            {
                CustomerId = requestDto.CustomerId,
                CarId = requestDto.CarId,
                IsBookedByEmployee = requestDto.IsBookedByEmployee,
                EmployeebookerId = requestDto.EmployeebookerId,
                Startdate = requestDto.Startdate,
                Enddate = requestDto.Enddate,
                PromocodeId = promocode?.PromocodeId,
                TotalPrice = totalPrice,
                BookingStatus = "Pending", // Initial status
                PaymentId = 0, // Will be updated after payment is created
                Subtotal = subtotal
            };

            var addedBooking = await _bookingRepository.AddAsync(bookingEntity);

            // After booking is added, update Payment with BookingId and get PaymentId
            if (requestDto.PaymentMethod?.ToLowerInvariant() == "creditcard" || requestDto.PaymentMethod?.ToLowerInvariant() == "cash")
            {
                var payment = await _paymentManager.GetPaymentsByBookingIdAsync(addedBooking.BookingId);
                if (payment.Any())
                {
                    addedBooking.PaymentId = payment.First().PaymentId;
                    await _bookingRepository.UpdateAsync(addedBooking);

                    //same as paymnent
                    await _carRepository.SetCarAvailabilityAsync(requestDto.CarId, false); // Car is no longer available
                    addedBooking = await _bookingRepository.GetBookingByIdAsync(addedBooking.BookingId); // Refresh to get PaymentId
                }
                else
                {
                    await _bookingRepository.DeleteAsync(addedBooking); // Rollback booking if payment record not found
                    return null; // Indicate failure
                }
            }
            else
            {
                addedBooking.BookingStatus = "Reserved"; // If not paying immediately
                await _bookingRepository.UpdateAsync(addedBooking);
            }

            return _mapper.Map<BookingDto>(addedBooking);
        }

        private decimal CalculateTotalPrice(int carId, DateOnly startDate, DateOnly endDate)
        {
            // Implement logic to calculate total price based on car and duration
            // This is a placeholder
            TimeSpan duration = endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue);
            return 50 * (decimal)duration.Days; // Example: $50 per day
        }

        private decimal ApplyPromocode(decimal price, Promocode promocode)
        {
            if (promocode != null && promocode.IsActive && promocode.ValidUntil >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            {
                return price * (1 - (promocode.DiscountPercentage / 100));
            }
            else
            {
                // If promocode is null, not active, or expired, return original price
                return price;
            }
        }

        public async Task<bool> DeleteBookingAsync(DeleteBookingRequestDto requestDto)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(requestDto.BookingId);
            if (booking == null)
                return false;

            if (booking.Startdate <= DateOnly.FromDateTime(DateTime.UtcNow))
                return false; // Cannot delete booking that has already started

            await _bookingRepository.DeleteAsync(booking);
            await _carRepository.SetCarAvailabilityAsync(booking.CarId, true); // Car becomes available again
            return true;
        }

        // Print document logic will be implemented later, requiring access to
        // Customer, Employee, Car, and Booking information.
        //add async before
        public Task<bool> PrintBookingDocumentAsync(int bookingId)
        {
            // Implementation will come later
            return Task.FromResult(true);
        }
    }

    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<Booking, BookingDto>().ReverseMap();
            // Note: MakeBookingRequestDto and DeleteBookingRequestDto are request models, handled manually.
        }
    }

}