using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Application.Managers
{
    public class BookingManager
    {

        private readonly IBookingRepository _bookingRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICarRepository _carRepository;
        private readonly IPromocodeRepository _promocodeRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly StripePaymentManager _stripePaymentManager;
        private readonly IMapper _mapper;
        private readonly ILogger<BookingManager> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingManager(
            IEmployeeRepository employeeRepository,
            IBookingRepository bookingRepository,
            ICustomerRepository customerRepository,
            ICarRepository carRepository,
            IPromocodeRepository promocodeRepository,
            IPaymentRepository paymentRepository,
            StripePaymentManager stripePaymentManager,
            IMapper mapper,
            UserManager<IdentityUser> userManager,
            ILogger<BookingManager> logger)
        {
            _employeeRepository = employeeRepository;
            _bookingRepository = bookingRepository;
            _customerRepository = customerRepository;
            _carRepository = carRepository;
            _userManager = userManager;
            _promocodeRepository = promocodeRepository;
            _paymentRepository = paymentRepository;
            _stripePaymentManager = stripePaymentManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BookingDto?> MakeBookingAsync(MakeBookingRequestDto requestDto, string loggedInUserId)
        {
            _logger.LogInformation("=== MakeBookingAsync START ===");
            _logger.LogInformation("LoggedInUserId: {UserId}", loggedInUserId);
            _logger.LogInformation("DTO: {@Dto}", requestDto);

            var user = await _userManager.FindByIdAsync(loggedInUserId);
            if (user == null)
            {
                _logger.LogWarning("Booking failed: user not found.");
                return null;
            }

            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");

            // 🔹 Set CustomerId if user is customer
            if (isCustomer)
            {
                var customerEntity = (await _customerRepository.GetAllAsync())
                    .FirstOrDefault(c => c.aspNetUserId == loggedInUserId);
                _logger.LogInformation("Customer is booking with customer id if this print them custome ris null");
                _logger.LogInformation("Customer is booking with customer id", customerEntity.UserId);
                _logger.LogInformation("Customer", customerEntity);
                if (customerEntity == null)
                {
                    _logger.LogWarning("Booking failed: No customer found for user {UserId}", loggedInUserId);
                    return null;
                }

                requestDto.CustomerId = customerEntity.UserId;
                _logger.LogInformation("✅ Auto-assigned CustomerId: {CustomerId}", requestDto.CustomerId);
            }

            // 🔹 Validate customer
            var customer = await _customerRepository.GetByIdAsync(requestDto.CustomerId);
            if (customer == null || !customer.IsVerified || !customer.Isactive)
            {
                _logger.LogWarning("Booking failed: Invalid customer [null: {Null}, verified: {Verified}, active: {Active}]",
                    customer == null, customer?.IsVerified, customer?.Isactive);
                return null;
            }

            // 🔹 Validate car
            var car = await _carRepository.GetByIdAsync(requestDto.CarId);
            if (car == null || !car.IsAvailable)
            {
                _logger.LogWarning("Booking failed: Car not found or unavailable.");
                return null;
            }

            var existingBookings = await _bookingRepository.GetBookingsByCarIdAsync(requestDto.CarId);
            var conflictingBookings = existingBookings
                .Where(b => IsBlockingStatus(b.BookingStatus)
                            && DatesOverlap(b.Startdate, b.Enddate, requestDto.Startdate, requestDto.Enddate))
                .ToList();

            if (conflictingBookings.Any())
            {
                var firstConflict = conflictingBookings.OrderBy(b => b.Startdate).First();
                _logger.LogWarning(
                    "Booking failed: Car {CarId} has {ConflictCount} conflicting bookings. First conflict between {ExistingStart} and {ExistingEnd}",
                    requestDto.CarId,
                    conflictingBookings.Count,
                    firstConflict.Startdate,
                    firstConflict.Enddate);
                return null;
            }

            // 🔹 Validate promocode
            Promocode? promocode = null;
            if (!string.IsNullOrEmpty(requestDto.Promocode))
            {
                promocode = await _promocodeRepository.GetByCodeAsync(requestDto.Promocode);
                if (promocode == null || !promocode.IsActive || promocode.ValidUntil < DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    _logger.LogWarning("Booking warning: Promocode invalid or expired.");
                    promocode = null;
                }
            }

            // 🔹 Calculate price
            decimal subtotal = CalculateTotalPrice(requestDto.CarId, requestDto.Startdate, requestDto.Enddate);
            decimal totalPrice = promocode != null ? ApplyPromocode(subtotal, promocode) : subtotal;

            // 🔹 Set employee booker if employee or admin
            int? employeeBookerIntId = null;
            bool isBookedByEmployee = isAdmin || isEmployee;

            if (isBookedByEmployee)
            {
                var emp = (await _employeeRepository.GetAllAsync()).FirstOrDefault(e => e.aspNetUserId == loggedInUserId);
                if (emp != null)
                {
                    employeeBookerIntId = emp.EmployeeId;
                    _logger.LogInformation("✅ Booking by employee. EmployeeId: {EmpId}", emp.EmployeeId);
                }
            }

            // 🔹 Create booking entity (without payment yet)
            var booking = new Booking
            {
                CustomerId = requestDto.CustomerId,
                CarId = requestDto.CarId,
                Startdate = requestDto.Startdate,
                Enddate = requestDto.Enddate,
                PromocodeId = promocode?.PromocodeId,
                TotalPrice = totalPrice,
                BookingStatus = "Pending",
                Subtotal = subtotal,
                IsBookedByEmployee = isBookedByEmployee,
                EmployeebookerId = isBookedByEmployee ? employeeBookerIntId : null,
                PaymentId = null
            };

            // Save booking first to generate BookingId
            var addedBooking = await _bookingRepository.AddAsync(booking);

            // 🔹 Create Stripe payment intent so the client can complete payment on Stripe
            var paymentIntent = await _stripePaymentManager.CreatePaymentIntentForBookingAsync(new StripePaymentIntentRequestDto
            {
                BookingId = addedBooking.BookingId,
                Currency = string.IsNullOrWhiteSpace(requestDto.Currency) ? null : requestDto.Currency,
                ReceiptEmail = string.IsNullOrWhiteSpace(requestDto.ReceiptEmail) ? user.Email : requestDto.ReceiptEmail
            });

            if (paymentIntent == null)
            {
                _logger.LogWarning("Rolling back booking {BookingId} because Stripe payment intent could not be created.", addedBooking.BookingId);
                await _bookingRepository.DeleteAsync(addedBooking);
                return null;
            }

            _logger.LogInformation("✅ Booking created with ID: {BookingId}", addedBooking.BookingId);
            var bookingDto = _mapper.Map<BookingDto>(addedBooking);
            bookingDto.StripePaymentIntentId = paymentIntent.PaymentIntentId;
            bookingDto.StripeClientSecret = paymentIntent.ClientSecret;
            bookingDto.StripeAmount = paymentIntent.Amount;
            bookingDto.StripeCurrency = paymentIntent.Currency;

            return bookingDto;
        }


        private static bool DatesOverlap(DateOnly existingStart, DateOnly existingEnd, DateOnly newStart, DateOnly newEnd)
        {
            return existingStart <= newEnd && existingEnd >= newStart;
        }

        private static bool IsBlockingStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return true;
            }

            return !status.Equals("cancelled", StringComparison.OrdinalIgnoreCase)
                && !status.Equals("rejected", StringComparison.OrdinalIgnoreCase);
        }


        private decimal CalculateTotalPrice(int carId, DateOnly startDate, DateOnly endDate)
        {
            TimeSpan duration = endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue);
            return 50 * (decimal)duration.Days;
        }

        private decimal ApplyPromocode(decimal price, Promocode promocode)
        {
            if (promocode != null && promocode.IsActive && promocode.ValidUntil >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            {
                return price * (1 - (promocode.DiscountPercentage / 100));
            }
            return price;
        }

        public async Task<BookingEditDto?> UpdateBookingAsync(BookingEditDto bookingDto)
        {
            _logger.LogInformation("Updating booking {Id}", bookingDto.BookingId);

            var booking = await _bookingRepository.GetBookingByIdAsync(bookingDto.BookingId);
            if (booking == null)
            {
                _logger.LogWarning("Booking {Id} not found", bookingDto.BookingId);
                return null;
            }

            _mapper.Map(bookingDto, booking);
            await _bookingRepository.UpdateAsync(booking);

            return _mapper.Map<BookingEditDto>(booking);
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
        public async Task<List<BookingDto>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return _mapper.Map<List<BookingDto>>(bookings);
        }
        public async Task<bool> DeleteBookingAsync(DeleteBookingRequestDto requestDto)
        {
            _logger.LogInformation("Deleting booking {Id}", requestDto.BookingId);

            var booking = await _bookingRepository.GetBookingByIdAsync(requestDto.BookingId);
            if (booking == null || booking.Startdate <= DateOnly.FromDateTime(DateTime.UtcNow))
                return false;

            // 🔍 Fetch payments for this booking
            var payments = await _paymentRepository.GetPaymentsByBookingIdAsync(booking.BookingId);

            // 🗑️ Delete all related payments first
            foreach (var payment in payments)
            {
                await _paymentRepository.DeleteAsync(payment);
            }

            // ✅ Then delete the booking
            await _bookingRepository.DeleteAsync(booking);

            return true;
        }


        public async Task<(DateOnly startDate, DateOnly endDate)> SuggestBookingDatesAsync(int carId)
        {
            var bookings = await _bookingRepository.GetBookingsByCarIdAsync(carId);
            var ordered = bookings.OrderBy(b => b.Startdate).ToList();

            DateOnly start = DateOnly.FromDateTime(DateTime.Today);

            foreach (var b in ordered)
            {
                if (start >= b.Startdate && start <= b.Enddate)
                {
                    start = b.Enddate.AddDays(1);
                    continue;
                }
                if (b.Startdate > start)
                {
                    break;
                }
            }

            var next = ordered.FirstOrDefault(b => b.Startdate > start);
            DateOnly end = start.AddDays(1);
            if (next != null)
            {
                var candidate = next.Startdate.AddDays(-1);
                if (candidate > start)
                {
                    end = candidate;
                }
            }

            return (start, end);
        }

        public Task<bool> PrintBookingDocumentAsync(int bookingId)
        {
            return Task.FromResult(true);
        }
    }

    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<Booking, BookingDto>().ReverseMap();

            CreateMap<Booking, BookingEditDto>().ReverseMap();

        }
    }
}