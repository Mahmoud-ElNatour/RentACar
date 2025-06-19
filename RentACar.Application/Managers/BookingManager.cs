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

namespace RentACar.Core.Managers
{
    public class BookingManager
    {

        private readonly IBookingRepository _bookingRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICarRepository _carRepository;
        private readonly IPromocodeRepository _promocodeRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly PaymentManager _paymentManager;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BookingManager> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingManager(
            IEmployeeRepository employeeRepository,
            IBookingRepository bookingRepository,
            ICustomerRepository customerRepository,
            ICarRepository carRepository,
            IPromocodeRepository promocodeRepository,
            IPaymentMethodRepository paymentMethodRepository,
            IPaymentRepository paymentRepository,
            PaymentManager paymentManager,
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
            _paymentMethodRepository = paymentMethodRepository;
            _paymentRepository = paymentRepository;
            _paymentManager = paymentManager;
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

            // 🔹 Validate payment method
            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(requestDto.PaymentMethodId);
            if (paymentMethod == null)
            {
                _logger.LogWarning("Booking failed: Payment method not found.");
                return null;
            }

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
                PaymentId = 0
            };

            // Save booking first to generate BookingId
            var addedBooking = await _bookingRepository.AddAsync(booking);

            // 🔹 Create payment linked to the newly created booking
            var payment = new Payment
            {
                BookingId = addedBooking.BookingId,
                Amount = totalPrice,
                PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                PaymentMethod = paymentMethod.PaymentMethodName,
                Status = "done",
                CreditcardId = paymentMethod.PaymentMethodName.Equals("creditcard", StringComparison.OrdinalIgnoreCase)
                    ? requestDto.CreditcardId
                    : null
            };

            var addedPayment = await _paymentRepository.AddAsync(payment);

            // Link payment back to booking
            addedBooking.PaymentId = addedPayment.PaymentId;
            await _bookingRepository.UpdateAsync(addedBooking);

            await _carRepository.SetCarAvailabilityAsync(booking.CarId, false);
            _logger.LogInformation("✅ Booking created with ID: {BookingId}", addedBooking.BookingId);
            return _mapper.Map<BookingDto>(addedBooking);
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

        public async Task<BookingDto?> UpdateBookingAsync(BookingDto bookingDto)
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

            return _mapper.Map<BookingDto>(booking);
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

            await _bookingRepository.DeleteAsync(booking);
            await _carRepository.SetCarAvailabilityAsync(booking.CarId, true);
            return true;
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
        }
    }
}