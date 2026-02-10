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
            // 1. Recent Bookings (Last 5)
            // Use local function or inline logic for Days calculation in query?
            // EF Core 8 might support DateOnly subtraction difference in days. 
            // b.Enddate and b.Startdate are DateOnly. 
            // SQL standard: DATEDIFF(day, Start, End).
            // EF Core translation for DateOnly subtract might work. 
            // If not, we projected everything else.
            // Let's safe bet: Project raw dates and calculate Description in memory, BUT avoid fetching the whole tree.
            
            var recentBookingsData = await _dbContext.Bookings
                .OrderByDescending(b => b.BookingId)
                .Take(5)
                .Select(b => new 
                {
                    b.BookingId,
                    CarModel = b.Car != null ? b.Car.ModelName : "Car",
                    CarPlate = b.Car != null ? b.Car.PlateNumber : "Unknown",
                    CustomerName = b.Customer != null ? b.Customer.Name : "Unknown",
                    b.Startdate,
                    b.Enddate
                })
                .AsNoTracking()
                .ToListAsync();

            foreach (var b in recentBookingsData)
            {
                var days = b.Enddate.DayNumber - b.Startdate.DayNumber;
                if (days < 1) days = 1;

                recentActivities.Add(new RecentActivityDto
                {
                    Title = $"New Booking #{b.BookingId}",
                    Description = $"{b.CarModel} ({b.CarPlate}) for {days} days. Customer: {b.CustomerName}",
                    TimeAgo = b.Startdate.ToString("MMM dd"), 
                    Icon = "add_circle",
                    IconColorClass = "text-primary"
                });
            }

            // 2. Recent Payments (Last 5)
            var recentPaymentsData = await _dbContext.Payments
                .OrderByDescending(p => p.PaymentId)
                .Take(5)
                .Select(p => new
                {
                    p.Amount,
                    p.PaymentMethod,
                    CustomerName = p.Booking != null && p.Booking.Customer != null ? p.Booking.Customer.Name : "Unknown",
                    p.PaymentDate
                })
                .AsNoTracking()
                .ToListAsync();

            foreach (var p in recentPaymentsData)
            {
                recentActivities.Add(new RecentActivityDto
                {
                    Title = "Payment Received",
                    Description = $"{p.Amount:C} via {p.PaymentMethod}. Customer: {p.CustomerName}",
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
        [Authorize(Roles = "Admin,Employee")]
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

            var monthCountsData = await _dbContext.Bookings
                .Where(b => b.Startdate.Year == DateTime.UtcNow.Year)
                .GroupBy(b => b.Startdate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();
            
            var months = Enumerable.Range(1, 12).Select(m => monthCountsData.FirstOrDefault(x => x.Month == m)?.Count ?? 0).ToList();
            var processedBookingsCount = await _dbContext.Bookings.CountAsync(b => b.IsBookedByEmployee == true && b.EmployeebookerId == employee.EmployeeId);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var activeBookingsSystemWide = await _dbContext.Bookings.CountAsync(b => b.Enddate >= today && b.Startdate <= today);
            var unverifiedCustomersCount = await _dbContext.Customers.CountAsync(c => !c.IsVerified);
            var waitingBookingsCount = await _dbContext.Bookings.CountAsync(b => b.BookingStatus == "Pending");

            // Fetch Distinct Years
            var availableYears = await _dbContext.Bookings
                .Select(b => b.Startdate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            // Fetch Recent Pending Bookings
            var recentPending = await _dbContext.Bookings
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
                    Status = "Pending",
                    StatusColorClass = "bg-yellow-500/10 text-yellow-500"
                })
                .AsNoTracking()
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
                ProcessedBookings = processedBookingsCount,
                TotalCars = (await _carManager.BrowseAllCarsAsync()).Count,
                AvailableCars = (await _carManager.SearchCarsByFilterAsync(isAvailable: true)).Count,
                UnverifiedCustomers = unverifiedCustomersCount,
                WaitingBookings = waitingBookingsCount,
                ActiveBookingsSystemWide = activeBookingsSystemWide,
                MonthlyProcessedBookings = months,
                AvailableYears = availableYears,
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
            var customer = await _customerManager.GetCustomerByAspNetUserId(user.Id);
            if (customer == null) return RedirectToAction("Index", "Home");

            var bookings = await _dbContext.Bookings
                .Where(b => b.CustomerId == customer.UserId)
                .Select(b => new 
                {
                    b.BookingId,
                    b.Startdate,
                    b.Enddate,
                    b.TotalPrice,
                    b.Subtotal,
                    b.BookingStatus,
                    CarModel = b.Car != null ? b.Car.ModelName : "Unknown Car",
                    CarId = b.Car != null ? b.Car.CarId : 0,
                    CarYear = b.Car != null ? b.Car.ModelYear : 2023,
                    CarCategory = b.Car != null && b.Car.Category != null ? b.Car.Category.Name : "Standard",
                    CarPrice = b.Car != null ? b.Car.PricePerDay : 0
                })
                .AsNoTracking()
                .ToListAsync();

            var upcoming = bookings.Count(b => b.Startdate.ToDateTime(TimeOnly.MinValue) > DateTime.UtcNow);
            var totalSpent = bookings.Sum(b => b.TotalPrice);
            var discountSavings = bookings.Sum(b => (b.Subtotal ?? b.TotalPrice) - b.TotalPrice);

            var bestCategory = bookings
                .GroupBy(b => b.CarCategory)
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
                     // Use URL for lazy loading image
                     string imgUrl = b.CarId > 0 ? $"/api/Car/Image/{b.CarId}" : $"https://ui-avatars.com/api/?name={b.CarModel}&background=random";

                    return new CustomerDashboardBookingDto
                    {
                        BookingId = b.BookingId,
                        CarName = b.CarModel,
                        CarImage = imgUrl,
                        DateRange = $"{b.Startdate:MMM dd} - {b.Enddate:MMM dd}",
                        TotalPrice = b.TotalPrice,
                        Status = b.BookingStatus,
                        StatusColorClass = b.BookingStatus switch
                        {
                            "Pending" => "text-yellow-500 bg-yellow-500/10",
                            "Confirmed" => "text-sky-500 bg-sky-500/10",
                            "InProgress" => "text-purple-500 bg-purple-500/10",
                            "Completed" => "text-green-500 bg-green-500/10",
                            "Cancelled" => "text-red-500 bg-red-500/10",
                            "Rejected" => "text-red-500 bg-red-500/10",
                            "AwaitingReturn" => "text-orange-500 bg-orange-500/10",
                            _ => "text-gray-500 bg-gray-500/10"
                        }
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
                IsGoldMember = totalSpent > 1000, 

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
                string nbImg = nextBooking.CarId > 0 ? $"/api/Car/Image/{nextBooking.CarId}" : $"https://ui-avatars.com/api/?name={nextBooking.CarModel}&background=random";

                model.NextBooking = new CustomerDashboardBookingDto
                {
                    BookingId = nextBooking.BookingId,
                    CarName = nextBooking.CarModel,
                    CarImage = nbImg,
                    DateRange = $"{nextBooking.Startdate:MMM dd} - {nextBooking.Enddate:MMM dd}",
                    TotalPrice = nextBooking.TotalPrice,
                    Status = nextBooking.BookingStatus,
                    StatusColorClass = "text-gold-500", 
                    PickupDate = nextBooking.Startdate.ToDateTime(TimeOnly.MinValue),
                    ReturnDate = nextBooking.Enddate.ToDateTime(TimeOnly.MinValue),
                    PickupLocation = "Beirut Airport", 
                    ReturnLocation = "Beirut Airport",
                    CarYear = nextBooking.CarYear.ToString() + " Model",
                    CarType = nextBooking.CarCategory
                };
            }

            // 2. Car Categories
            model.CarCategories = await _dbContext.Cars
                .Where(c => c.Category != null)
                .Select(c => c.Category!.Name)
                .Distinct()
                .ToListAsync();

            // 3. Suggested Cars (Sidebar)
            // Need to project to avoid blob fetching here too.
            // Logic: 2 Available cars of BestCategory
            
            var suggestedQuery = _dbContext.Cars.AsQueryable();
            if (!string.IsNullOrEmpty(bestCategory))
            {
                 // Join or navigation check
                 suggestedQuery = suggestedQuery.Where(c => c.Category != null && c.Category.Name == bestCategory);
            }
            suggestedQuery = suggestedQuery.Where(c => c.IsAvailable);
            
            var suggestedCars = await suggestedQuery
                .Take(2)
                .Select(c => new 
                {
                    c.CarId,
                    c.ModelName,
                    c.PricePerDay
                })
                .AsNoTracking()
                .ToListAsync();

            model.SuggestedCars = suggestedCars.Select(c => {
                 string cImg = $"/api/Car/Image/{c.CarId}";

                 return new CustomerDashboardSuggestedCarDto 
                 {
                     CarId = c.CarId,
                     ModelName = c.ModelName,
                     PricePerDay = c.PricePerDay ?? 0,
                     ImageUrl = cImg,
                     Transmission = "Automatic", 
                     FuelType = "Petrol" 
                 };
            }).ToList();

            return View("~/Views/Dashboard/Customer.cshtml", model);
        }
    }
}
