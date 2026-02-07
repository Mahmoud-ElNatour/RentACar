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
using RentACar.Core.Constants;

namespace RentACar.Application.Managers
{
    public class PaymentManager
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IPromocodeRepository _promocodeRepository; // Added
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Services.IStripePaymentService _stripePaymentService;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentManager> _logger;
        private readonly AuditLogManager _auditLogManager;
        private readonly EmailManager _emailManager;

        public PaymentManager(
            IPaymentRepository paymentRepository,
            IBookingRepository bookingRepository,
            IPaymentMethodRepository paymentMethodRepository,
            IPromocodeRepository promocodeRepository, // Added
            UserManager<IdentityUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            Services.IStripePaymentService stripePaymentService,
            IMapper mapper,
            ILogger<PaymentManager> logger,
            AuditLogManager auditLogManager,
            EmailManager emailManager)
        {
            _paymentRepository = paymentRepository;
            _bookingRepository = bookingRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _promocodeRepository = promocodeRepository; // Added
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _stripePaymentService = stripePaymentService;
            _mapper = mapper;
            _logger = logger;
            _auditLogManager = auditLogManager;
            _emailManager = emailManager;
        }

        public async Task<MakePaymentResultDto?> MakePaymentByCustomerAsync(MakePaymentRequestDto paymentDto, int customerUserId)
        {
            _logger.LogInformation("Customer {Id} making payment for booking {Booking}", customerUserId, paymentDto.BookingId);

            var booking = await _bookingRepository.GetByIdAsync(paymentDto.BookingId);
            if (booking == null || booking.CustomerId != customerUserId)
                return null;

            var existingPayments = await _paymentRepository.GetPaymentsByBookingIdAsync(paymentDto.BookingId);
            if (existingPayments.Any())
            {
                var latestPayment = existingPayments
                    .OrderByDescending(p => p.PaymentDate)
                    .ThenByDescending(p => p.PaymentId)
                    .First();

                // ✅ If already paid, don't redirect
                if (string.Equals(latestPayment.Status, PaymentStatus.Paid, StringComparison.OrdinalIgnoreCase))
                {
                    var paidDto = _mapper.Map<PaymentDto>(latestPayment);

                    var methodId = await ResolvePaymentMethodIdAsync(latestPayment.PaymentMethod);
                    if (methodId.HasValue) paidDto.PaymentMethodId = methodId.Value;

                    return new MakePaymentResultDto
                    {
                        Payment = paidDto,
                        RequiresRedirect = false,
                        RedirectUrl = null
                    };
                }

                // ✅ If NOT paid yet (unpaid), continue to create Stripe session (redirect)
                // so DO NOT return here
            }


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
                    Status = PaymentStatus.Pending,
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
                    Status = PaymentStatus.Paid
                };
                await _paymentRepository.AddAsync(payment);
                await _auditLogManager.LogEventAsync("Payment.Created", "Payment", payment.PaymentId.ToString(), $"Customer payment of {payment.Amount:C} via {payment.PaymentMethod}", null, "Success");
                
                // 📨 Send Payment Success Email (Cash/Direct)
                booking = await _bookingRepository.GetByIdAsync(paymentDto.BookingId);
                var customer = await _userManager.FindByIdAsync(booking?.Customer?.aspNetUserId);
                 // Need to reload booking with customer includes usually, but let's try to get customer from repository or user manager
                // Use existing data or fetch
                if (booking != null) {
                     // We need customer details.
                     // The booking object from _bookingRepository.GetByIdAsync usually includes basic or we can get it.
                     // If relations are not included, we might need to fetch customer.
                     // In MakePaymentByCustomerAsync, we passed customerUserId (int).
                     // We can get email from user manager if we have userId string? 
                     // customerUserId is int.
                     // We can find IdentityUser via CustomerRepository?
                     // Let's assume we can fetch it.
                     // Or just use Logged In User? MakePaymentByCustomerAsync is called by customer.
                     // But let's be safe.
                     // payment.BookingId is available.
                }
                // Simpler: assume we can get Customer email.
                // Re-fetch booking with includes or fetch Customer separately.
                // Assuming lazy loading or repository includes.
                // Let's rely on booking logic or fetching.
                
                var custUser = await _userManager.FindByIdAsync((await _bookingRepository.GetByIdAsync(paymentDto.BookingId)).Customer.aspNetUserId);
                if (custUser != null) {
                     await _emailManager.SendPaymentSuccessEmail(custUser.Email, (await _bookingRepository.GetByIdAsync(paymentDto.BookingId)).Customer.Name, payment, await _bookingRepository.GetByIdAsync(paymentDto.BookingId));
                }

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
                // Credit card payments are now handled via Stripe externally
                var payment = new Payment
                {
                    BookingId = paymentDto.BookingId,
                    Amount = paymentDto.Amount,
                    PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    PaymentMethod = paymentMethod.PaymentMethodName,
                    Status = PaymentStatus.Paid
                };

                await _paymentRepository.AddAsync(payment);

                await _auditLogManager.LogEventAsync("Payment.Created", "Payment", payment.PaymentId.ToString(), $"Employee recorded payment of {payment.Amount:C}", null, "Success");

                // 📨 Send Payment Success Email (Employee Recorded)
                var booking = await _bookingRepository.GetByIdAsync(paymentDto.BookingId);
                if (booking != null) {
                    var cust = booking.Customer; // Assuming Include or Lazy loading. Repository often returns includes.
                    if (cust != null) {
                        var custUser = await _userManager.FindByIdAsync(cust.aspNetUserId);
                        if (custUser != null)
                             await _emailManager.SendPaymentSuccessEmail(custUser.Email, cust.Name, payment, booking);
                    }
                }

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
                    Status = PaymentStatus.Paid
                };

                await _paymentRepository.AddAsync(payment);
                await _auditLogManager.LogEventAsync("Payment.Created", "Payment", payment.PaymentId.ToString(), $"Employee recorded cash payment: {payment.Amount:C}", null, "Success");
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

            if (string.Equals(payment.Status, PaymentStatus.Paid, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Stripe webhook received for already completed payment {PaymentId}", paymentId);
                return true;
            }

            payment.Status = PaymentStatus.Paid;
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
            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
            if (booking != null)
            {
                await UpdateBookingStatusForPaymentAsync(booking, payment.Status);
                
                // 📨 Send Payment Success Email (Stripe Webhook)
                var cust = booking.Customer;
                if (cust != null) {
                     var custUser = await _userManager.FindByIdAsync(cust.aspNetUserId);
                     if (custUser != null)
                          await _emailManager.SendPaymentSuccessEmail(custUser.Email, cust.Name, payment, booking);
                }
            }
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

        public async Task<PaymentResultDto> GetPaymentsAsync(PaymentFilterDto filter)
        {
            // Base query for Counting (No Includes)
            var query = _paymentRepository.Query().AsNoTracking();

            query = ApplyFilters(query, filter);

            var totalCount = await query.CountAsync();

            // items query (With Includes)
            // We need to re-apply filters to the include-heavy query, or attach includes to the filtered query?
            // Attaching includes to an existing query is possible.
            // But ApplyFilters might have added Where clauses.
            // The cleanest way is to add Includes to the ALREADY FILTERED query?
            // No, Includes should be added before specific selects, but EF Core allows adding Include after Where.
            // Let's add Includes now.
            
            var listQuery = query; // No includes needed for projection

            // 3. Sorting
            // Re-assign listQuery for sorting
            IQueryable<Payment> sortedQuery = listQuery;
            if (!string.IsNullOrWhiteSpace(filter.SortColumn))
            {
                bool asc = filter.SortDirection?.ToLower() == "asc";
                switch (filter.SortColumn.ToLower())
                {
                    case "paymentid": sortedQuery = asc ? sortedQuery.OrderBy(p => p.PaymentId) : sortedQuery.OrderByDescending(p => p.PaymentId); break;
                    case "bookingid": sortedQuery = asc ? sortedQuery.OrderBy(p => p.BookingId) : sortedQuery.OrderByDescending(p => p.BookingId); break;
                    case "amount": sortedQuery = asc ? sortedQuery.OrderBy(p => p.Amount) : sortedQuery.OrderByDescending(p => p.Amount); break;
                    case "paymentdate": sortedQuery = asc ? sortedQuery.OrderBy(p => p.PaymentDate) : sortedQuery.OrderByDescending(p => p.PaymentDate); break;
                    case "status": sortedQuery = asc ? sortedQuery.OrderBy(p => p.Status) : sortedQuery.OrderByDescending(p => p.Status); break;
                    case "customername": 
                        sortedQuery = asc 
                            ? sortedQuery.OrderBy(p => p.Booking != null && p.Booking.Customer != null ? p.Booking.Customer.Name : "") 
                            : sortedQuery.OrderByDescending(p => p.Booking != null && p.Booking.Customer != null ? p.Booking.Customer.Name : ""); 
                        break;
                    default: sortedQuery = sortedQuery.OrderByDescending(p => p.PaymentId); break;
                }
            }
            else
            {
                sortedQuery = sortedQuery.OrderByDescending(p => p.PaymentId);
            }

            // 4. Pagination
            var items = await sortedQuery
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new PaymentListDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,
                    CustomerId = p.Booking != null ? p.Booking.CustomerId : 0,
                    CustomerName = p.Booking != null && p.Booking.Customer != null ? p.Booking.Customer.Name : null,
                    CustomerUsername = p.Booking != null && p.Booking.Customer != null && p.Booking.Customer.User != null ? p.Booking.Customer.User.UserName : null,
                    CarModel = p.Booking != null && p.Booking.Car != null ? p.Booking.Car.ModelName : null,
                    CarPlate = p.Booking != null && p.Booking.Car != null ? p.Booking.Car.PlateNumber : null,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    PaymentMethodName = p.PaymentMethod,
                    Status = p.Status,
                    PaymentProvider = p.PaymentProvider,
                    BookingStatus = p.Booking != null ? p.Booking.BookingStatus : null,
                    BookingTotal = p.Booking != null ? p.Booking.TotalPrice : null,
                    BookingSubtotal = p.Booking != null ? p.Booking.Subtotal : null,
                    PromocodeName = p.Booking != null && p.Booking.Promocode != null ? p.Booking.Promocode.Name : null,
                    PromocodeDiscountPercentage = p.Booking != null && p.Booking.Promocode != null ? p.Booking.Promocode.DiscountPercentage : null
                })
                .ToListAsync();

            return new PaymentResultDto
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize),
                Stats = null // Stats separate
            };
        }

        public async Task<PaymentStatsDto> GetPaymentStatsAsync(PaymentFilterDto filter)
        {
             var query = _paymentRepository.Query().AsNoTracking();
             
             // Optimized Stats Filter: Ignore heavy text search, only apply Status/Date
             if (!string.IsNullOrWhiteSpace(filter.Status))
             {
                 if (filter.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                     query = query.Where(p => p.Status == "Paid" || p.Status == "Done");
                 else
                     query = query.Where(p => p.Status == filter.Status);
             }

             if (filter.StartDate.HasValue) query = query.Where(p => p.PaymentDate >= filter.StartDate.Value);
             if (filter.EndDate.HasValue) query = query.Where(p => p.PaymentDate <= filter.EndDate.Value);

             // Note: We skip SearchTerm for stats to ensure query remains fast and reflects "Global" or "Date-Range" stats 
             // rather than narrowed down single-row stats, which is often what users prefer for dashboard cards.

            var statsGroup = await query
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count(), Sum = g.Sum(p => p.Amount) })
                .ToListAsync();

            return new PaymentStatsDto
            {
                TotalRevenue = statsGroup.Where(x => string.Equals(x.Status, PaymentStatus.Paid, StringComparison.OrdinalIgnoreCase)).Sum(x => x.Sum),
                PendingAmount = statsGroup.Where(x => string.Equals(x.Status, PaymentStatus.Pending, StringComparison.OrdinalIgnoreCase)).Sum(x => x.Sum),
                PendingCount = statsGroup.Where(x => string.Equals(x.Status, PaymentStatus.Pending, StringComparison.OrdinalIgnoreCase)).Sum(x => x.Count),
                SuccessCount = statsGroup.Where(x => string.Equals(x.Status, PaymentStatus.Paid, StringComparison.OrdinalIgnoreCase)).Sum(x => x.Count),
                RefundAmount = statsGroup.Where(x => string.Equals(x.Status, PaymentStatus.Refunded, StringComparison.OrdinalIgnoreCase)).Sum(x => x.Sum),
                TotalCount = statsGroup.Sum(x => x.Count)
            };
        }

        private IQueryable<Payment> ApplyFilters(IQueryable<Payment> query, PaymentFilterDto filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                if (int.TryParse(term, out var num))
                {
                    query = query.Where(p => p.PaymentId == num || p.BookingId == num);
                }
                else
                {
                    // Case-insensitive like search
                    var pattern = $"%{term}%";
                    query = query.Where(p => 
                        p.Booking != null && 
                        p.Booking.Customer != null && 
                        EF.Functions.Like(p.Booking.Customer.Name, pattern));
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                if (filter.Status.Equals(PaymentStatus.Paid, StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.Status == PaymentStatus.Paid);
                }
                else
                {
                    query = query.Where(p => p.Status == filter.Status);
                }
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(p => p.PaymentDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(p => p.PaymentDate <= filter.EndDate.Value);
            }
            return query;
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

            // Credit card validation removed - payments handled via Stripe

            var payment = new Payment
            {
                BookingId = dto.BookingId,
                Amount = dto.Amount,
                PaymentDate = dto.PaymentDate,
                PaymentMethod = paymentMethod.PaymentMethodName,
                Status = NormalizePaymentStatus(dto.Status),
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

            // Credit card validation removed - payments handled via Stripe

            existing.Amount = dto.Amount;
            existing.PaymentDate = dto.PaymentDate;
            existing.PaymentMethod = paymentMethod.PaymentMethodName;
            existing.Status = NormalizePaymentStatus(dto.Status);
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
            var subtotal = payment.Booking?.Subtotal;
            var promocode = payment.Booking?.Promocode;
            var discountPercentage = promocode?.DiscountPercentage;

            return new PaymentDetailsDto
            {
                PaymentId = payment.PaymentId,
                BookingId = payment.BookingId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethodName = payment.PaymentMethod,
                Status = payment.Status,
                PaymentProvider = payment.PaymentProvider,
                PaymentProviderSessionId = payment.PaymentProviderSessionId,
                CustomerName = payment.Booking?.Customer?.Name,
                CustomerUsername = payment.Booking?.Customer?.User?.UserName,
                BookingStatus = payment.Booking?.BookingStatus,
                BookingSubtotal = subtotal,
                PromocodeName = promocode?.Name,
                PromocodeDiscountPercentage = discountPercentage,
                BookingHasDriver = payment.Booking?.HasDriver,
                BookingDriverDailyFee = payment.Booking?.DriverDailyFee,
                CarExtraDriverFeePerDay = payment.Booking?.Car?.ExtraDriverFeePerDay,
                CarPricePerDay = payment.Booking?.Car?.PricePerDay,
                CarModel = payment.Booking?.Car?.ModelName,
                CarPlateNumber = payment.Booking?.Car?.PlateNumber,
                BookingStartDate = payment.Booking?.Startdate,
                BookingEndDate = payment.Booking?.Enddate
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

            if (paymentStatus.Equals(PaymentStatus.Cancelled, StringComparison.OrdinalIgnoreCase) ||
                paymentStatus.Equals(PaymentStatus.Pending, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (paymentStatus.Equals(PaymentStatus.Paid, StringComparison.OrdinalIgnoreCase))
            {
                // Logic: If Today is start date -> InProgress, Else -> Confirmed
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                string newStatus = (booking.Startdate <= today) ? BookingStatus.InProgress : BookingStatus.Confirmed;
                
                // Only update if current status is Pending (or we want to re-confirm?)
                // Assuming we move from Pending -> Confirmed/InProgress
                if (string.Equals(booking.BookingStatus, BookingStatus.Pending, StringComparison.OrdinalIgnoreCase))
                {
                    booking.BookingStatus = newStatus;
                    await _bookingRepository.UpdateAsync(booking);
                }
            }
        }

        public async Task<bool> MarkPaymentCancelledAsync(int paymentId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                _logger.LogWarning("Stripe cancel received for missing payment {PaymentId}", paymentId);
                return false;
            }

            if (string.Equals(payment.Status, PaymentStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            payment.Status = PaymentStatus.Cancelled;
            await _paymentRepository.UpdateAsync(payment);
            
            // 📨 Send Payment Cancelled Email
            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
            if (booking != null && booking.Customer != null) {
                 var custUser = await _userManager.FindByIdAsync(booking.Customer.aspNetUserId);
                 if (custUser != null)
                    await _emailManager.SendPaymentCancelledEmail(custUser.Email, booking.Customer.Name, payment.Amount);
            }
            
            
            return true;
        }

        public async Task<bool> MarkPaymentFailedAsync(int paymentId, string failureReason)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null) return false;

            if (string.Equals(payment.Status, PaymentStatus.Failed, StringComparison.OrdinalIgnoreCase)) return true;

            payment.Status = PaymentStatus.Failed;
            await _paymentRepository.UpdateAsync(payment);

            // 📨 Send Payment Failed Email
            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
            if (booking != null && booking.Customer != null)
            {
                 var custUser = await _userManager.FindByIdAsync(booking.Customer.aspNetUserId);
                 if (custUser != null)
                    await _emailManager.SendPaymentFailedEmail(custUser.Email, booking.Customer.Name, booking.BookingId, payment.Amount);
            }
            
            await _auditLogManager.LogEventAsync("PaymentFailed", "Payment", paymentId.ToString(), $"Payment failed: {failureReason}", null, "Failed");
            return true;
        }

        public async Task<bool> ApplyPromocodeToPaymentAsync(int paymentId, int promocodeId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null) return false;

            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
            if (booking == null) return false;

            var promo = await _promocodeRepository.GetByIdAsync(promocodeId);
            if (promo == null || !promo.IsActive) return false;

            // Validate date
            if (promo.ValidUntil.HasValue && promo.ValidUntil.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                return false;
            }

            // Apply to booking
            booking.PromocodeId = promocodeId;
            
            // Recalculate Total
            decimal subtotal = booking.Subtotal ?? 0m;
            decimal discount = subtotal * (promo.DiscountPercentage / 100m);
            booking.TotalPrice = subtotal - discount;
            
            await _bookingRepository.UpdateAsync(booking);

            // Update Payment Amount
            payment.Amount = booking.TotalPrice;
            await _paymentRepository.UpdateAsync(payment);
            
            // UsageCount not in entity, skipping

            await _auditLogManager.LogEventAsync("Payment.PromocodeApplied", "Payment", paymentId.ToString(), $"Applied promo {promo.Name} ({promo.DiscountPercentage}%) to booking {booking.BookingId}", null, "Success");

            return true;
        }

        public async Task<StripeCheckoutSessionDto> CreateCheckoutSessionForPaymentAsync(Payment payment)
        {
            try
            {
                var session = await CreateStripeCheckoutSessionAsync(payment);

                if (!string.IsNullOrWhiteSpace(session.SessionId))
                {
                    payment.PaymentProviderSessionId = session.SessionId;
                }

                if (!string.IsNullOrWhiteSpace(session.PaymentIntentId))
                {
                    payment.PaymentProviderPaymentIntentId = session.PaymentIntentId;
                }

                if (!string.IsNullOrWhiteSpace(session.SessionId) || !string.IsNullOrWhiteSpace(session.PaymentIntentId))
                {
                    payment.PaymentProvider = "Stripe";
                    await _paymentRepository.UpdateAsync(payment);
                }

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Stripe Checkout Session for Payment {PaymentId}", payment.PaymentId);
                return new StripeCheckoutSessionDto
                {
                    CheckoutUrl = null,
                    RawResponse = $"Error: {ex.Message}"
                };
            }
        }

        private static string? NormalizePaymentStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return status;
            }

            if (status.Equals("done", StringComparison.OrdinalIgnoreCase))
            {
                return "Paid";
            }

            if (status.Equals("pending", StringComparison.OrdinalIgnoreCase))
            {
                return "Unpaid";
            }

            if (status.Equals("rejected", StringComparison.OrdinalIgnoreCase))
            {
                return "Cancelled";
            }

            return status;
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
