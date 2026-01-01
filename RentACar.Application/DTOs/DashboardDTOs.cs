using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentACar.Application.DTOs
{
    public class AdminDashboardViewModel
    {
        public int TotalCars { get; set; }
        public int AvailableCars { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public decimal IncomeThisMonth { get; set; }
        public decimal IncomeThisYear { get; set; }
        public decimal ExpectedRevenue { get; set; }
        public decimal SalariesToPay { get; set; }
        public List<int> MonthlyBookings { get; set; } = new();
        public List<int> AvailableYears { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
    }

    public class RecentActivityDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty; // Store formatted string directly or use DateTime and format in View
        public string Icon { get; set; } = "notifications"; // Material icon name
        public string IconColorClass { get; set; } = "text-primary"; // Tailwind color class
    }

    public class EmployeeDashboardViewModel
    {
        public int ProcessedBookings { get; set; }
        public int TotalCars { get; set; }
        public int AvailableCars { get; set; } // Fleet Available
        public int UnverifiedCustomers { get; set; }
        public int WaitingBookings { get; set; } // Pending Approvals
        public int ActiveBookingsSystemWide { get; set; }
        public List<int> MonthlyProcessedBookings { get; set; } = new();
        
        public List<EmployeeDashboardBookingDto> RecentPendingBookings { get; set; } = new();
        public List<EmployeeDashboardCustomerDto> UnverifiedCustomersList { get; set; } = new();
    }

    public class EmployeeDashboardBookingDto
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerImage { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string DateRange { get; set; } = string.Empty; // e.g. "Oct 24 - Oct 28"
        public string Status { get; set; } = string.Empty;
        public string StatusColorClass { get; set; } = "text-yellow-500 bg-yellow-500/10";
    }

    public class EmployeeDashboardCustomerDto
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string IssueText { get; set; } = string.Empty; // e.g. "ID Missing", "Pending DL"
        public string IssueColorClass { get; set; } = "text-red-400";
        public string IssueIcon { get; set; } = "error";
    }

    public class CustomerDashboardViewModel
    {
        public int TotalBookings { get; set; }
        public int UpcomingBookings { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal DiscountSavings { get; set; }
        public string? BestCategory { get; set; }
        public List<int> MonthlyBookings { get; set; } = new();
    }
}
