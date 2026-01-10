using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.Managers;
using RentACar.Infrastructure.Data;
using RentACar.Application.DTOs;

namespace RentACar.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly CarManager _carManager;
        private readonly CustomerManager _customerManager;
        private readonly EmployeeManager _employeeManager;
        private readonly RentACarDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailManager _emailManager;

        public DashboardController(
            CarManager carManager,
            CustomerManager customerManager,
            EmployeeManager employeeManager,
            RentACarDbContext dbContext,
            UserManager<IdentityUser> userManager,
            EmailManager emailManager)
        {
            _carManager = carManager;
            _customerManager = customerManager;
            _employeeManager = employeeManager;
            _dbContext = dbContext;
            _userManager = userManager;
            _emailManager = emailManager;
        }

        [HttpGet("~/Dashboard")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
                return await Admin();
            if (User.IsInRole("Employee"))
                return await Employee();
            if (User.IsInRole("Customer"))
                return await Customer();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("~/Dashboard/Admin")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Admin()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var now = DateTime.UtcNow;

            // Stats
            var totalCars = await _dbContext.Cars.CountAsync();
            var availableCars = await _dbContext.Cars.CountAsync(c => c.IsAvailable);
            var totalCustomers = await _dbContext.Customers.CountAsync();
            var totalEmployees = await _dbContext.Employees.CountAsync();
            var totalBookings = await _dbContext.Bookings.CountAsync();
            var activeBookings = await _dbContext.Bookings.CountAsync(b => b.Enddate >= today && b.Startdate <= today);

            // Financials
            var payments = await _dbContext.Payments.ToListAsync();
            var incomeMonth = payments
                .Where(p => p.PaymentDate.Year == now.Year && p.PaymentDate.Month == now.Month)
                .Sum(p => p.Amount);
            
            var incomeYear = payments
                .Where(p => p.PaymentDate.Year == now.Year)
                .Sum(p => p.Amount);
            
            var employees = await _employeeManager.GetAllEmployees();
            var salaries = employees.Sum(e => e.Salary ?? 0m);
            var expectedRevenue = incomeYear - salaries;

            // Chart Data
            var monthly = await _dbContext.Bookings
                .Where(b => b.Startdate.Year == now.Year)
                .GroupBy(b => b.Startdate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();
            var monthCounts = Enumerable.Range(1, 12).Select(m => monthly.FirstOrDefault(x => x.Month == m)?.Count ?? 0).ToList();

            // Calculate Available Years
            var firstBookingDate = await _dbContext.Bookings.MinAsync(b => (DateOnly?)b.Startdate);
            var startYear = firstBookingDate?.Year ?? now.Year;
            var availableYears = Enumerable.Range(startYear, now.Year - startYear + 1).OrderByDescending(y => y).ToList();

            // Recent Activity
            var recentActivities = new List<RecentActivityDto>();

            // 1. Recent Bookings (Last 5)
            var recentBookings = await _dbContext.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .OrderByDescending(b => b.BookingId)
                .Take(5)
                .ToListAsync();

            foreach (var b in recentBookings)
            {
                // Calculate days roughly
                var days = b.Enddate.DayNumber - b.Startdate.DayNumber;
                if (days < 1) days = 1;

                recentActivities.Add(new RecentActivityDto
                {
                    Title = $"New Booking #{b.BookingId}",
                    Description = $"{b.Car?.ModelName ?? "Car"} ({b.Car?.PlateNumber ?? "Unknown"}) for {days} days. Customer: {b.Customer?.Name ?? "Unknown"}",
                    TimeAgo = b.Startdate.ToString("MMM dd"), 
                    Icon = "add_circle",
                    IconColorClass = "text-primary"
                });
            }

            // 2. Recent Payments (Last 5)
            var recentPayments = await _dbContext.Payments
                .Include(p => p.Booking).ThenInclude(b => b.Customer)
                .OrderByDescending(p => p.PaymentId)
                .Take(5)
                .ToListAsync();

            foreach (var p in recentPayments)
            {
                recentActivities.Add(new RecentActivityDto
                {
                    Title = "Payment Received",
                    Description = $"{p.Amount:C} via {p.PaymentMethod}. Customer: {p.Booking?.Customer?.Name ?? "Unknown"}",
                    TimeAgo = p.PaymentDate.ToString("MMM dd"),
                    Icon = "payments",
                    IconColorClass = "text-blue-400"
                });
            }

            var model = new AdminDashboardViewModel
            {
                TotalCars = totalCars,
                AvailableCars = availableCars,
                TotalCustomers = totalCustomers,
                TotalEmployees = totalEmployees,
                TotalBookings = totalBookings,
                ActiveBookings = activeBookings,
                IncomeThisMonth = incomeMonth,
                IncomeThisYear = incomeYear,
                SalariesToPay = salaries,
                ExpectedRevenue = expectedRevenue,
                MonthlyBookings = monthCounts,
                AvailableYears = availableYears,
                RecentActivities = recentActivities
            };
            return View("~/Views/Dashboard/Admin.cshtml", model);
        }

        [HttpGet("GetChartData")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetChartData(int year)
        {
            var monthly = await _dbContext.Bookings
                .Where(b => b.Startdate.Year == year)
                .GroupBy(b => b.Startdate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();
            
            var monthCounts = Enumerable.Range(1, 12)
                .Select(m => monthly.FirstOrDefault(x => x.Month == m)?.Count ?? 0)
                .ToList();

            return Ok(monthCounts);
        }
        [HttpPost("~/Dashboard/SendReminderToUnverified")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> SendReminderToUnverified()
        {
            var count = await _emailManager.SendReminderToAllUnverifiedAsync();
            return Json(new { success = true, message = $"Sent reminders to {count} unverified customers." });
        }

        [HttpPost("~/Dashboard/SendReminderToCustomer")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> SendReminderToCustomer(int id)
        {
            var success = await _emailManager.SendReminderToCustomerAsync(id);
            if (success)
            {
                return Json(new { success = true, message = "Reminder email sent successfully." });
            }
            return Json(new { success = false, message = "Failed to send reminder. Check logs or customer status." });
        }

        [HttpGet("~/Dashboard/Employee")]
        [Authorize(Roles = "Employee")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Employee()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");
            var employee = (await _employeeManager.GetAllEmployees()).FirstOrDefault(e => e.aspNetUserId == user.Id);
            if (employee == null) return RedirectToAction("Index", "Home");

            var bookings = await _dbContext.Bookings
                .Where(b => b.IsBookedByEmployee == true && b.EmployeebookerId == employee.EmployeeId)
                .ToListAsync();
            var monthCounts = bookings
                .GroupBy(b => b.Startdate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToList();
            var months = Enumerable.Range(1, 12).Select(m => monthCounts.FirstOrDefault(x => x.Month == m)?.Count ?? 0).ToList();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var activeBookingsSystemWide = await _dbContext.Bookings.CountAsync(b => b.Enddate >= today && b.Startdate <= today);
            var unverifiedCustomersCount = await _dbContext.Customers.CountAsync(c => !c.IsVerified);
            var waitingBookingsCount = await _dbContext.Bookings.CountAsync(b => b.BookingStatus == "Pending");

            // Fetch Recent Pending Bookings
            var recentPending = await _dbContext.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .Where(b => b.BookingStatus == "Pending")
                .OrderByDescending(b => b.BookingId)
                .Take(5)
                .Select(b => new EmployeeDashboardBookingDto
                {
                    BookingId = b.BookingId,
                    CustomerName = b.Customer != null ? b.Customer.Name : "Unknown",
                    CustomerImage = $"https://ui-avatars.com/api/?name={(b.Customer != null ? b.Customer.Name : "User")}&background=random", // Placeholder
                    CarModel = b.Car != null ? b.Car.ModelName : "Unknown Car",
                    DateRange = $"{b.Startdate:MMM dd} - {b.Enddate:MMM dd}",
                    Status = "Pending Review",
                    StatusColorClass = "bg-yellow-500/10 text-yellow-500"
                })
                .ToListAsync();

            // Fetch Unverified Customers
            var unverifiedList = await _dbContext.Customers
                .Where(c => !c.IsVerified)
                .Take(5)
                .Select(c => new EmployeeDashboardCustomerDto
                {
                    CustomerId = c.UserId,
                    Name = c.Name,
                    ImageUrl = $"https://ui-avatars.com/api/?name={c.Name}&background=random",
                    IssueText = "ID Missing", // Logic could be more complex based on actual missing fields
                    IssueColorClass = "text-red-400",
                    IssueIcon = "id_card"
                })
                .ToListAsync();

            var model = new EmployeeDashboardViewModel
            {
                ProcessedBookings = bookings.Count,
                TotalCars = (await _carManager.BrowseAllCarsAsync()).Count,
                AvailableCars = (await _carManager.SearchCarsByFilterAsync(isAvailable: true)).Count,
                UnverifiedCustomers = unverifiedCustomersCount,
                WaitingBookings = waitingBookingsCount,
                ActiveBookingsSystemWide = activeBookingsSystemWide,
                MonthlyProcessedBookings = months,
                RecentPendingBookings = recentPending,
                UnverifiedCustomersList = unverifiedList
            };
            return View("~/Views/Dashboard/Employee.cshtml", model);
        }

        [HttpGet("~/Dashboard/Customer")]
        [Authorize(Roles = "Customer")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Customer()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");
            var customer = (await _customerManager.GetAllCustomers()).FirstOrDefault(c => c.aspNetUserId == user.Id);
            if (customer == null) return RedirectToAction("Index", "Home");

            var bookings = await _dbContext.Bookings
                .Where(b => b.CustomerId == customer.UserId)
                .Include(b => b.Car)
                .ThenInclude(c => c.Category)
                .ToListAsync();
            var upcoming = bookings.Count(b => b.Startdate.ToDateTime(TimeOnly.MinValue) > DateTime.UtcNow);
            var totalSpent = bookings.Sum(b => b.TotalPrice);
            var discountSavings = bookings.Sum(b => (b.Subtotal ?? b.TotalPrice) - b.TotalPrice);

            var bestCategory = bookings
                .GroupBy(b => b.Car.Category?.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefault()?.Category;

            var monthCounts = bookings
                .GroupBy(b => b.Startdate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToList();
            var months = Enumerable.Range(1, 12).Select(m => monthCounts.FirstOrDefault(x => x.Month == m)?.Count ?? 0).ToList();

            var recentBookings = bookings
                .OrderByDescending(b => b.BookingId)
                .Take(5)
                .Select(b => {
                    string imgData = "";
                    if (b.Car != null && b.Car.CarImage != null && b.Car.CarImage.Length > 0)
                    {
                        string base64 = Convert.ToBase64String(b.Car.CarImage);
                        imgData = $"data:image/png;base64,{base64}";
                    }
                    else
                    {
                         // Placeholder
                         imgData = $"https://ui-avatars.com/api/?name={(b.Car != null ? b.Car.ModelName : "Car")}&background=random";
                    }

                    return new CustomerDashboardBookingDto
                    {
                        BookingId = b.BookingId,
                        CarName = b.Car != null ? b.Car.ModelName : "Unknown Car",
                        CarImage = imgData,
                        DateRange = $"{b.Startdate:MMM dd} - {b.Enddate:MMM dd}",
                        TotalPrice = b.TotalPrice,
                        Status = b.BookingStatus,
                        StatusColorClass = b.BookingStatus == "Accepted" ? "text-green-500 bg-green-500/10" :
                                           b.BookingStatus == "Pending" ? "text-yellow-500 bg-yellow-500/10" :
                                           "text-red-500 bg-red-500/10"
                    };
                })
                .ToList();

            var model = new CustomerDashboardViewModel
            {
                // Basic Stats
                TotalBookings = bookings.Count,
                UpcomingBookings = upcoming,
                TotalSpent = totalSpent,
                DiscountSavings = discountSavings,
                BestCategory = bestCategory,
                
                // User Details
                CustomerName = customer.Name,
                CustomerImageUrl = $"https://ui-avatars.com/api/?name={customer.Name}&background=d4af35&color=14120b",
                IsGoldMember = totalSpent > 1000, // Simple logic for Gold Status

                // Charts & Lists
                MonthlyBookings = months,
                RecentBookings = recentBookings
            };

            // 1. Next Booking (Hero Card)
            var nextBooking = bookings
                .Where(b => b.Startdate.ToDateTime(TimeOnly.MinValue) > DateTime.UtcNow)
                .OrderBy(b => b.Startdate)
                .FirstOrDefault();

            if (nextBooking != null)
            {
                string nbImg = "";
                if (nextBooking.Car != null && nextBooking.Car.CarImage != null && nextBooking.Car.CarImage.Length > 0)
                {
                    nbImg = $"data:image/png;base64,{Convert.ToBase64String(nextBooking.Car.CarImage)}";
                }
                else
                {
                    nbImg = $"https://ui-avatars.com/api/?name={(nextBooking.Car?.ModelName ?? "Car")}&background=random";
                }

                model.NextBooking = new CustomerDashboardBookingDto
                {
                    BookingId = nextBooking.BookingId,
                    CarName = nextBooking.Car?.ModelName ?? "Unknown Car",
                    CarImage = nbImg,
                    DateRange = $"{nextBooking.Startdate:MMM dd} - {nextBooking.Enddate:MMM dd}",
                    TotalPrice = nextBooking.TotalPrice,
                    Status = nextBooking.BookingStatus,
                    StatusColorClass = "text-gold-500", // Hero usually gold or white
                    PickupDate = nextBooking.Startdate.ToDateTime(TimeOnly.MinValue),
                    ReturnDate = nextBooking.Enddate.ToDateTime(TimeOnly.MinValue),
                    PickupLocation = "Beirut Airport", // Hardcoded for now or fetch from booking if available
                    ReturnLocation = "Beirut Airport",
                    CarYear = (nextBooking.Car?.ModelYear ?? 2023).ToString() + " Model",
                    CarType = nextBooking.Car?.Category?.Name ?? "Premium"
                };
            }

            // 2. Car Categories (for Quick Book)
            // Assuming Category is accessible via Car or directly. Since we don't have _categoryManager injected here and unsure of DbSet, use Cars.
            model.CarCategories = await _dbContext.Cars
                .Where(c => c.Category != null)
                .Select(c => c.Category.Name)
                .Distinct()
                .ToListAsync();

            // 3. Suggested Cars (Sidebar)
            // Logic: 2 Available cars of BestCategory, excluding current bookings
            var suggestedQuery = _dbContext.Cars.Include(c => c.Category).Where(c => c.IsAvailable);
            if (!string.IsNullOrEmpty(bestCategory))
            {
                suggestedQuery = suggestedQuery.Where(c => c.Category.Name == bestCategory);
            }
            
            var suggestedCars = await suggestedQuery.Take(2).ToListAsync();
            model.SuggestedCars = suggestedCars.Select(c => {
                 string cImg = "";
                 if(c.CarImage != null && c.CarImage.Length > 0) cImg = $"data:image/png;base64,{Convert.ToBase64String(c.CarImage)}";
                 else cImg = $"https://ui-avatars.com/api/?name={c.ModelName}&background=random";

                 return new CustomerDashboardSuggestedCarDto 
                 {
                     CarId = c.CarId,
                     ModelName = c.ModelName,
                     PricePerDay = c.PricePerDay ?? 0,
                     ImageUrl = cImg,
                     Transmission = "Automatic", // Default as property doesn't exist on entity
                     FuelType = "Petrol" // Placeholder
                 };
            }).ToList();

            return View("~/Views/Dashboard/Customer.cshtml", model);
        }
    }
}

