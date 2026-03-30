using Microsoft.EntityFrameworkCore;
using RentACar.Application.DTOs.Support;
using RentACar.Application.Services;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RentACar.Application.Managers
{
    public class AiSupportContextManager
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICarRepository _carRepository;
        private readonly ICategoryRepository _categoryRepository; // Added field
        private readonly IPromocodeRepository _promocodeRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly ITripRepository _tripRepository;
        private readonly Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager;

        public AiSupportContextManager(
            ICustomerRepository customerRepository, 
            IBookingRepository bookingRepository,
            IPaymentRepository paymentRepository,
            ICarRepository carRepository,
            ICategoryRepository categoryRepository, // Added param
            IPromocodeRepository promocodeRepository,
            IPaymentMethodRepository paymentMethodRepository,
            IDriverRepository driverRepository,
            ITripRepository tripRepository,
            Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager)
        {
            _customerRepository = customerRepository;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
            _carRepository = carRepository;
            _categoryRepository = categoryRepository; // Assigned
            _promocodeRepository = promocodeRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _driverRepository = driverRepository;
            _tripRepository = tripRepository;
            _userManager = userManager;
        }

        public async Task<AiSupportContext> GetContextForCustomerAsync(int customerId)
        {
            var context = new AiSupportContext();
            context.GlobalContext = await BuildGlobalContextAsync();
            context.UserContext = await BuildUserContextAsync(customerId);
            return context;
        }

        private async Task<AiGlobalContext> BuildGlobalContextAsync()
        {
            var global = new AiGlobalContext();
            
            // --- 0. Company Info (Placeholder) ---
            global.Company = new AiCompanyInfo
            {
                Email = "info@lebanondrive.com", 
                PhoneNumber = "+961 70 123 456",
                Address = "Hamra Main St, Beirut, Lebanon"
            };

            // --- 1. Status Definitions ---
            global.StatusDefinitions = new Dictionary<string, string>
            {
                { "Pending", "Booking created, waiting for successful payment." },
                { "Confirmed", "Payment done. Waiting for customer to sign contract and pickup car." },
                { "InProgress", "Customer has picked up the car. Currently rented." },
                { "AwaitingReturn", "Booking overdue. Waiting for customer to return car." },
                { "Completed", "Car returned. Booking cycle finished." },
                { "Cancelled", "Booking cancelled." },
                { "Rejected", "Booking rejected by admin." }
            };

            // --- 2. Fetch Inventory & Availability ---
            // Fetch Categories first
            var categories = await _categoryRepository.GetAllActiveAsync();
            if (categories.Any())
            {
                global.AllCategories = categories
                    .Select(c => c.Name)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();
            }

            // Fetch Cars
            var allCars = await _carRepository.GetAllAsync(); 
            var availableCars = allCars.Where(c => c.IsAvailable).ToList();

            // Fetch ALL bookings
            var allBookings = await _bookingRepository.GetAllAsync();
            var today = DateOnly.FromDateTime(DateTime.Now);
            var limitDate = today.AddDays(45); // Look 45 days ahead

            var activeBookings = allBookings
                .Where(b => b.Enddate >= today && b.Startdate <= limitDate) 
                .Where(b => b.BookingStatus == "Confirmed" || b.BookingStatus == "InProgress" || b.BookingStatus == "Pending")
                .ToList();

            if (availableCars.Any())
            {
                // Summaries
                var summary = availableCars
                    .GroupBy(c => 
                    {
                        // Safe Category Lookup
                        var catName = c.Category?.Name;
                        if (string.IsNullOrEmpty(catName) && c.CategoryId > 0)
                        {
                            catName = categories.FirstOrDefault(cat => cat.CategoryId == c.CategoryId)?.Name;
                        }
                        return catName ?? "General";
                    })
                    .Select(g => 
                    {
                        var minPrice = g.Min(c => c.PricePerDay);
                        var models = string.Join(", ", g.Select(c => c.ModelName).Distinct().Take(3));
                        return $"{g.Key}: Starts at ${minPrice}/day. Models: {models}...";
                    })
                    .ToList();
                global.InventorySummary = summary;

                // Detailed Fleet Availability
                foreach (var car in availableCars)
                {
                    var carBookings = activeBookings
                        .Where(b => b.CarId == car.CarId)
                        .OrderBy(b => b.Startdate)
                        .ToList();

                    var color = !string.IsNullOrEmpty(car.Color) ? car.Color : "N/A";
                    
                    var specs = $"{car.SeatsCapacity} Seats, {car.TransmissionType}, {car.FuelType}";
                    var features = new List<string>();
                    if (car.HasInfotainmentScreen) features.Add("Infotainment");
                    if (car.HasGPS) features.Add("GPS");
                    if (car.HasSunroof) features.Add("Sunroof");
                    if (car.HasParkingSensors) features.Add("Sensors");
                    if (car.HasRearCamera) features.Add("Camera");
                    if (features.Any()) specs += $" [{string.Join(", ", features)}]";

                    // Safe Category Lookup
                    var catName = car.Category?.Name;
                    if (string.IsNullOrEmpty(catName) && car.CategoryId > 0)
                    {
                        catName = categories.FirstOrDefault(cat => cat.CategoryId == car.CategoryId)?.Name;
                    }
                    catName = catName ?? "General";

                    if (carBookings.Any())
                    {
                        var statusParts = new List<string>();
                        foreach (var b in carBookings)
                        {
                            statusParts.Add($"Booked {b.Startdate:MM/dd}-{b.Enddate:MM/dd} ({b.BookingStatus})");
                        }
                        global.FleetAvailability.Add($"{car.ModelName} ({catName}, Color: {color}, {specs}): {string.Join(", ", statusParts)}");
                    }
                    else
                    {
                        global.FleetAvailability.Add($"{car.ModelName} ({catName}, Color: {color}, {specs}): Available Now (Price: ${car.PricePerDay}/day)");
                    }
                }
            }
            else
            {
                global.InventorySummary.Add("No cars currently available.");
                global.FleetAvailability.Add("No cars in fleet.");
            }

            // --- 3. Promos & Payments ---
            var promos = await _promocodeRepository.GetAllAsync();
            var activePromos = promos.Where(p => p.IsActive && p.ValidUntil > today).ToList();
            
            global.ActivePromotions = activePromos.Any() 
                ? activePromos.Select(p => $"Code: {p.Name} ({p.DiscountPercentage}% off) - {p.Description}").ToList() 
                : new List<string> { "No active promotions." };

            var methods = await _paymentMethodRepository.GetAllAsync();
            global.PaymentMethods = methods.Where(m => m.IsActive).Select(m => m.PaymentMethodName).Any() 
                ? methods.Where(m => m.IsActive).Select(m => m.PaymentMethodName).ToList() 
                : new List<string> { "Credit Card" };

            return global;
        }

        private async Task<AiUserContext> BuildUserContextAsync(int customerId)
        {
            var userCtx = new AiUserContext { CustomerId = customerId };

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null) return userCtx;

            userCtx.Name = customer.Name;
            userCtx.IsVerified = customer.IsVerified;

            // Fetch Identity User for Email/Phone
            if (!string.IsNullOrEmpty(customer.aspNetUserId))
            {
                var identityUser = await _userManager.FindByIdAsync(customer.aspNetUserId);
                if (identityUser != null)
                {
                    userCtx.Email = identityUser.Email ?? "";
                    userCtx.PhoneNumber = identityUser.PhoneNumber ?? "";
                }
            }

            var bookings = await _bookingRepository.GetBookingsByCustomerIdAsync(customerId);
            
            var bookingInfos = new List<AiBookingInfo>();
            foreach (var b in bookings.OrderByDescending(x => x.BookingId))
            {
                bookingInfos.Add(new AiBookingInfo
                {
                    BookingId = b.BookingId,
                    Status = b.BookingStatus,
                    StartDate = b.Startdate.ToDateTime(TimeOnly.MinValue),
                    EndDate = b.Enddate.ToDateTime(TimeOnly.MinValue),
                    TotalPrice = b.TotalPrice,     
                    CarName = b.Car?.ModelName ?? "Unknown Car",
                    PlateNumber = b.Car?.PlateNumber ?? "N/A",
                    Color = b.Car?.Color ?? "N/A",
                    Category = b.Car?.Category?.Name ?? "Standard",
                    SeatsCapacity = b.Car?.SeatsCapacity ?? 5,
                    TransmissionType = b.Car?.TransmissionType.ToString() ?? "N/A",
                    FuelType = b.Car?.FuelType.ToString() ?? "N/A",
                    LuggageCapacity = b.Car?.LuggageCapacity,
                    HasInfotainmentScreen = b.Car?.HasInfotainmentScreen ?? false,
                    HasGPS = b.Car?.HasGPS ?? false,
                    HasSunroof = b.Car?.HasSunroof ?? false,
                    HasParkingSensors = b.Car?.HasParkingSensors ?? false,
                    HasRearCamera = b.Car?.HasRearCamera ?? false,
                    PickupAddress = b.PickupAddress ?? "N/A",
                    PickupLocationLabel = b.PickupLocationLabel ?? "N/A",
                    PickupDateTime = b.PickupDateTime ?? DateTime.MinValue,
                    HasDriver = b.HasDriver,
                    DriverFee = b.DriverDailyFee ?? 0m   
                });
            }

            userCtx.RecentBookings = bookingInfos.Take(5).ToList(); 
            var active = bookingInfos.FirstOrDefault(b => b.Status == "Confirmed" || b.Status == "InProgress" || b.Status == "Pending");
            userCtx.ActiveBooking = active;

            foreach (var b in userCtx.RecentBookings)
            {
                var payments = await _paymentRepository.GetPaymentsByBookingIdAsync(b.BookingId);
                foreach (var p in payments)
                {
                    userCtx.RecentPayments.Add(new AiPaymentInfo
                    {
                        PaymentId = p.PaymentId, 
                        Amount = p.Amount,
                        Status = p.Status,
                        Date = p.PaymentDate.ToDateTime(TimeOnly.MinValue), 
                        Method = p.PaymentMethod
                    });
                }
            }
            
            return userCtx;
        }
    }
}
