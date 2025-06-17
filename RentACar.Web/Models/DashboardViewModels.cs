namespace RentACar.Web.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalCars { get; set; }
        public int AvailableCars { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalBookings { get; set; }
        public decimal IncomeThisMonth { get; set; }
        public decimal IncomeThisYear { get; set; }
        public decimal ExpectedRevenue { get; set; }
        public decimal SalariesToPay { get; set; }
        public List<int> MonthlyBookings { get; set; } = new();
    }

    public class EmployeeDashboardViewModel
    {
        public int ProcessedBookings { get; set; }
        public int TotalCars { get; set; }
        public int AvailableCars { get; set; }
        public int UnverifiedCustomers { get; set; }
        public int WaitingBookings { get; set; }
        public List<int> MonthlyProcessedBookings { get; set; } = new();
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
