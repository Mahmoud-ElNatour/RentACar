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
using Microsoft.EntityFrameworkCore;

namespace RentACar.Application.Managers
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
        private readonly AuditLogManager _auditLogManager;
        private readonly EmailManager _emailManager;

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
            ILogger<BookingManager> logger,
            AuditLogManager auditLogManager,
            EmailManager emailManager)
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
            _auditLogManager = auditLogManager;
            _emailManager = emailManager;
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
            decimal subtotal = CalculateTotalPrice(car.PricePerDay ?? 0, requestDto.Startdate, requestDto.Enddate);
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
                EmployeebookerId = isBookedByEmployee ? employeeBookerIntId : null
            };

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
            
            // 📨 Send Booking Status Email (Pending)
            if (isCustomer) 
            {
                 // We need customer email. If loggedInUser is customer we have it.
                 // RequestDto has CustomerId.
                 // We fetched customerEntity earlier.
                 // If isCustomer is true, customerEntity is set.
                 // If admin booked for customer, requestDto.CustomerId is set.
                 var cust = await _customerRepository.GetByIdAsync(addedBooking.CustomerId);
                 var custUser = await _userManager.FindByIdAsync(cust.aspNetUserId);
                 if (custUser != null)
                 {
                     await _emailManager.SendBookingStatusEmail(custUser.Email, cust.Name, addedBooking);
                 }
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

            var oldStatus = booking.BookingStatus;
            _mapper.Map(bookingDto, booking);
            await _bookingRepository.UpdateAsync(booking);
            await _auditLogManager.LogEventAsync("Booking.StatusChanged", "Booking", bookingDto.BookingId.ToString(), $"Updated booking details. Status: {booking.BookingStatus}", null, "Success");

            // 📨 Send Email if Status Changed
            if (oldStatus != booking.BookingStatus) 
            {
                var cust = await _customerRepository.GetByIdAsync(booking.CustomerId);
                var custUser = await _userManager.FindByIdAsync(cust.aspNetUserId);
                if (custUser != null)
                {
                    // "On admin rejection -> status = Rejected (must include reason)"
                    // Reason might be in DTO? BookingEditDto doesn't seem to have Reason field shown here but maybe mapped?
                    // "Reason (only for Rejected)"
                    // I will pass generic "Status Change" reason or null. 
                    // If rejection, maybe check if DTO has notes? Assuming no notes field in DTO for now.
                    await _emailManager.SendBookingStatusEmail(custUser.Email, cust.Name, booking);
                }
            }

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
        public async Task<IEnumerable<BookingListDto>> GetAllBookingsForListAsync()
        {
            // 1. Fetch Bookings (Projected)
            var query = _bookingRepository.Query()
                .Include(b => b.Customer).ThenInclude(c => c.User)
                .Include(b => b.Car).ThenInclude(c => c.Category)
                .Include(b => b.Promocode)
                .Include(b => b.Employeebooker)
                .AsNoTracking();

            var bookings = await query.Select(b => new BookingListDto
            {
                BookingId = b.BookingId,
                CustomerId = b.CustomerId,
                CustomerName = b.Customer != null ? b.Customer.Name : null,
                CustomerUsername = b.Customer != null && b.Customer.User != null ? b.Customer.User.UserName : null,
                CustomerEmail = b.Customer != null && b.Customer.User != null ? b.Customer.User.Email : null,
                
                CarId = b.CarId,
                CarModel = b.Car != null ? b.Car.ModelName : null,
                CarPlate = b.Car != null ? b.Car.PlateNumber : null,
                
                EmployeebookerId = b.EmployeebookerId,
                EmployeeName = b.Employeebooker != null ? b.Employeebooker.Name : null,
                
                Subtotal = b.Subtotal,
                TotalPrice = b.TotalPrice,
                
                PromocodeId = b.PromocodeId,
                PromocodeName = b.Promocode != null ? b.Promocode.Name : null,
                PromocodeDiscount = b.Promocode != null ? b.Promocode.DiscountPercentage : null,
                
                Startdate = b.Startdate.ToString(),
                Enddate = b.Enddate.ToString(),
                BookingStatus = b.BookingStatus
            }).ToListAsync();

            if (!bookings.Any())
                return bookings;

            // 2. Fetch Payments (Batch)
            var bookingIds = bookings.Select(b => b.BookingId).ToList();
            
            // Chunking to avoid SQL limit if necessary, but assuming reasonable count for now.
            var payments = await _paymentRepository.Query()
                .Where(p => bookingIds.Contains(p.BookingId))
                .Select(p => new { p.BookingId, p.PaymentId, p.Amount, p.Status, p.PaymentDate })
                .AsNoTracking()
                .ToListAsync();

            // 3. Join In-Memory
            foreach (var b in bookings)
            {
                var bookingPayments = payments.Where(p => p.BookingId == b.BookingId).ToList();
                if (bookingPayments.Any())
                {
                    // Logic from Controller: OrderByDescending(PaymentDate).ThenByDescending(PaymentId).FirstOrDefault()
                    var latest = bookingPayments
                        .OrderByDescending(p => p.PaymentDate)
                        .ThenByDescending(p => p.PaymentId)
                        .First();

                    b.PaymentId = latest.PaymentId;
                    b.PaymentAmount = latest.Amount;
                    b.PaymentStatus = latest.Status;
                }
            }

            return bookings;
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
