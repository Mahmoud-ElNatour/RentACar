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
using RentACar.Web.Models;

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

        public DashboardController(
            CarManager carManager,
            CustomerManager customerManager,
            EmployeeManager employeeManager,
            RentACarDbContext dbContext,
            UserManager<IdentityUser> userManager)
        {
            _carManager = carManager;
            _customerManager = customerManager;
            _employeeManager = employeeManager;
            _dbContext = dbContext;
            _userManager = userManager;
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
            var totalCars = (await _carManager.BrowseAllCarsAsync()).Count;
            var availableCars = (await _carManager.SearchCarsByFilterAsync(isAvailable: true)).Count;
            var totalCustomers = (await _customerManager.GetAllCustomers()).Count;
            var employees = await _employeeManager.GetAllEmployees();
            var totalEmployees = employees.Count;
            var totalBookings = await _dbContext.Bookings.CountAsync();

            var payments = await _dbContext.Payments.ToListAsync();
            var now = DateTime.UtcNow;
            var incomeMonth = payments
                .Where(p => p.PaymentDate.Year == now.Year && p.PaymentDate.Month == now.Month)
                .Sum(p => p.Amount);
            var incomeYear = payments.Where(p => p.PaymentDate.Year == now.Year).Sum(p => p.Amount);
            var salaries = employees.Sum(e => e.Salary ?? 0m);
            var expectedRevenue = incomeYear - salaries;

            var monthly = await _dbContext.Bookings
                .GroupBy(b => b.Startdate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .OrderBy(g => g.Month)
                .ToListAsync();
            var monthCounts = Enumerable.Range(1, 12).Select(m => monthly.FirstOrDefault(x => x.Month == m)?.Count ?? 0).ToList();

            var model = new AdminDashboardViewModel
            {
                TotalCars = totalCars,
                AvailableCars = availableCars,
                TotalCustomers = totalCustomers,
                TotalEmployees = totalEmployees,
                TotalBookings = totalBookings,
                IncomeThisMonth = incomeMonth,
                IncomeThisYear = incomeYear,
                SalariesToPay = salaries,
                ExpectedRevenue = expectedRevenue,
                MonthlyBookings = monthCounts
            };
            return View("~/Views/Dashboard/Admin.cshtml", model);
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

            var unverifiedCustomers = await _dbContext.Customers.CountAsync(c => !c.IsVerified);
            var waitingBookings = await _dbContext.Bookings.CountAsync(b => b.BookingStatus == "Pending");

            var model = new EmployeeDashboardViewModel
            {
                ProcessedBookings = bookings.Count,
                TotalCars = (await _carManager.BrowseAllCarsAsync()).Count,
                AvailableCars = (await _carManager.SearchCarsByFilterAsync(isAvailable: true)).Count,
                UnverifiedCustomers = unverifiedCustomers,
                WaitingBookings = waitingBookings,
                MonthlyProcessedBookings = months
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

            var model = new CustomerDashboardViewModel
            {
                TotalBookings = bookings.Count,
                UpcomingBookings = upcoming,
                TotalSpent = totalSpent,
                DiscountSavings = discountSavings,
                BestCategory = bestCategory,
                MonthlyBookings = months
            };
            return View("~/Views/Dashboard/Customer.cshtml", model);
        }
    }
}

