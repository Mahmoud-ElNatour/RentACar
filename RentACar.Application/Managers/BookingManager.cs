using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Application.Services;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Application.Managers
{
    public class BookingManager
    {
        private readonly IGoogleGeocodingService _geocodingService;
        private readonly IBookingRepository _bookingRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IDriverAvailabilityRepository _driverAvailabilityRepository;
        private readonly ICarRepository _carRepository;
        private readonly IPromocodeRepository _promocodeRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly PaymentManager _paymentManager;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BookingManager> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AuditLogManager _auditLogManager;

        public BookingManager(
            IEmployeeRepository employeeRepository,
            IDriverRepository driverRepository,
            IDriverAvailabilityRepository driverAvailabilityRepository,
            IBookingRepository bookingRepository,
            ICustomerRepository customerRepository,
            ICarRepository carRepository,
            IPromocodeRepository promocodeRepository,
            IPaymentMethodRepository paymentMethodRepository,
            IPaymentRepository paymentRepository,
            PaymentManager paymentManager,
            IGoogleGeocodingService geocodingService,
            IMapper mapper,
            UserManager<IdentityUser> userManager,
            ILogger<BookingManager> logger,
            AuditLogManager auditLogManager
            )
        {
            _employeeRepository = employeeRepository;
            _driverRepository = driverRepository;
            _driverAvailabilityRepository = driverAvailabilityRepository;
            _bookingRepository = bookingRepository;
            _customerRepository = customerRepository;
            _carRepository = carRepository;
            _geocodingService = geocodingService;
            _userManager = userManager;
            _promocodeRepository = promocodeRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _paymentRepository = paymentRepository;
            _paymentManager = paymentManager;
            _mapper = mapper;
            _logger = logger;
            _auditLogManager = auditLogManager;
        }

        public async Task<BookingCreationResultDto?> MakeBookingAsync(MakeBookingRequestDto requestDto, string loggedInUserId)
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

            int? assignedDriverId = null;
            decimal? driverDailyFee = null;
            if (requestDto.HasDriver)
            {
                driverDailyFee = requestDto.DriverDailyFee ?? 85m;
                assignedDriverId = await FindAvailableDriverAsync(requestDto.Startdate, requestDto.Enddate);
                if (!assignedDriverId.HasValue)
                {
                    _logger.LogWarning("Booking failed: No available driver found.");
                    return null;
                }
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
            decimal baseSubtotal = CalculateTotalPrice(car.PricePerDay ?? 0, requestDto.Startdate, requestDto.Enddate);
            decimal driverFeeTotal = requestDto.HasDriver && driverDailyFee.HasValue
                ? CalculateTotalPrice(driverDailyFee.Value, requestDto.Startdate, requestDto.Enddate)
                : 0m;
            decimal subtotal = baseSubtotal + driverFeeTotal;
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
                HasDriver = requestDto.HasDriver,
                DriverId = requestDto.HasDriver ? assignedDriverId : null,
                DriverDailyFee = requestDto.HasDriver ? driverDailyFee : null,
                PickupAddress = requestDto.PickupAddress,
                PickupLocationLabel = requestDto.PickupLocationName,
                PickupDateTime = requestDto.PickupDateTime
            };

            var requestLat = requestDto.PickupLatitude;
            var requestLng = requestDto.PickupLongitude;

            if (!requestLat.HasValue || !requestLng.HasValue)
            {
                _logger.LogWarning("Pickup pin missing for booking request.");
                throw new InvalidOperationException("MISSING_PICKUP_PIN");
            }

            booking.PickupLatitude = requestLat;
            booking.PickupLongitude = requestLng;


            // Save booking first to generate BookingId
            var addedBooking = await _bookingRepository.AddAsync(booking);

            var payableAmount = CalculatePayableAmount(totalPrice, paymentMethod.PaymentMethodName);

            // 🔹 Create payment linked to the newly created booking
            var payment = new Payment
            {
                BookingId = addedBooking.BookingId,
                Amount = payableAmount,
                PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                PaymentMethod = paymentMethod.PaymentMethodName,
                Status = "Unpaid",
                PaymentProvider = "Stripe",
                CreditcardId = paymentMethod.PaymentMethodName.Equals("creditcard", StringComparison.OrdinalIgnoreCase)
                    ? requestDto.CreditcardId
                    : null
            };

            var addedPayment = await _paymentRepository.AddAsync(payment);

            _logger.LogInformation("✅ Booking created with ID: {BookingId}", addedBooking.BookingId);
            
            await _auditLogManager.LogEventAsync("Booking.Created", "Booking", addedBooking.BookingId.ToString(), $"Created new booking for Car {addedBooking.CarId}", null, "Success");

            if (addedBooking.HasDriver && addedBooking.DriverId.HasValue)
            {
                await _auditLogManager.LogEventAsync("Booking.DriverAssigned", "Booking", addedBooking.BookingId.ToString(), $"Driver {addedBooking.DriverId} assigned to booking.", null, "Success");
            }
            
            var session = await _paymentManager.CreateCheckoutSessionForPaymentAsync(addedPayment);
            if (string.IsNullOrWhiteSpace(session.CheckoutUrl))
            {
                _logger.LogWarning("Stripe checkout session missing URL for booking {BookingId}", addedBooking.BookingId);
            }

            return new BookingCreationResultDto
            {
                Booking = _mapper.Map<BookingDto>(addedBooking),
                RedirectUrl = session.CheckoutUrl,
                PaymentId = addedPayment.PaymentId
            };
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

            return !status.Equals("returned", StringComparison.OrdinalIgnoreCase)
                && !status.Equals("rejected", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<int?> FindAvailableDriverAsync(DateOnly startDate, DateOnly endDate)
        {
            var drivers = await _driverRepository.GetActiveAsync();
            if (!drivers.Any())
            {
                return null;
            }

            foreach (var driver in drivers)
            {
                var identityUser = await _userManager.FindByIdAsync(driver.AspNetUserId);
                if (identityUser == null || !await _userManager.IsInRoleAsync(identityUser, "Driver"))
                {
                    continue;
                }

                var driverBookings = await _bookingRepository.GetBookingsByDriverIdAsync(driver.DriverId);
                var hasConflict = driverBookings.Any(b =>
                    IsBlockingStatus(b.BookingStatus) &&
                    DatesOverlap(b.Startdate, b.Enddate, startDate, endDate));

                if (hasConflict)
                {
                    continue;
                }

                var availability = await _driverAvailabilityRepository.GetByDriverIdAsync(driver.DriverId);
                var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
                var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);
                var isUnavailable = availability.Any(a =>
                    !a.IsAvailable &&
                    a.StartDateTime <= endDateTime &&
                    a.EndDateTime >= startDateTime);

                if (isUnavailable)
                {
                    continue;
                }

                return driver.DriverId;
            }

            return null;
        }

        private static decimal CalculatePayableAmount(decimal totalPrice, string? paymentMethodName)
        {
            if (string.IsNullOrWhiteSpace(paymentMethodName))
            {
                return totalPrice;
            }

            if (paymentMethodName.Equals("cash", StringComparison.OrdinalIgnoreCase))
            {
                return Math.Round(totalPrice * 0.30m, 2, MidpointRounding.AwayFromZero);
            }

            return totalPrice;
        }


        private decimal CalculateTotalPrice(decimal pricePerDay, DateOnly startDate, DateOnly endDate)
        {
            TimeSpan duration = endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue);
            return pricePerDay * (decimal)duration.Days;
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
            await _auditLogManager.LogEventAsync("Booking.StatusChanged", "Booking", bookingDto.BookingId.ToString(), $"Updated booking details. Status: {booking.BookingStatus}", null, "Success");

            return _mapper.Map<BookingEditDto>(booking);
        }


        public async Task<List<BookingDto>> GetBookingHistoryAsync(int customerId)
        {
            var bookings = await _bookingRepository.GetBookingsByCustomerIdAsync(customerId);
            return _mapper.Map<List<BookingDto>>(bookings);
        }

        public async Task<bool> UpdateBookingStatusAsync(int bookingId, string status)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return false;
            }

            booking.BookingStatus = status;
            await _bookingRepository.UpdateAsync(booking);

            await _auditLogManager.LogEventAsync(
                "Booking.StatusChanged",
                "Booking",
                bookingId.ToString(),
                $"Driver updated status to {status} at {DateTime.UtcNow:O}",
                null,
                "Success");

            return true;
        }

        public async Task<List<BookingDto>> GetBookingsByEmployeeIdAsync(int employeeId)
        {
            var bookings = await _bookingRepository.GetBookingsByEmployeeIdAsync(employeeId);
            return _mapper.Map<List<BookingDto>>(bookings);
        }

        public async Task<List<BookingDto>> GetBookingsByDriverIdAsync(int driverId)
        {
            var bookings = await _bookingRepository.GetBookingsByDriverIdAsync(driverId);
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
            await _auditLogManager.LogEventAsync("Booking.Cancelled", "Booking", requestDto.BookingId.ToString(), "Deleted booking and related payments", null, "Success");

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
            CreateMap<BookingDto, BookingEditDto>().ReverseMap();
        }
    }
}
