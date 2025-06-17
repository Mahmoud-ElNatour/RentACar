namespace RentACar.Web.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalCars { get; set; }
        public int AvailableCars { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalBookings { get; set; }
        public List<int> MonthlyBookings { get; set; } = new();
    }

    public class EmployeeDashboardViewModel
    {
        public int EmployeeBookings { get; set; }
        public int TotalCars { get; set; }
        public int AvailableCars { get; set; }
        public List<int> MonthlyEmployeeBookings { get; set; } = new();
    }

    public class CustomerDashboardViewModel
    {
        public int TotalBookings { get; set; }
        public int UpcomingBookings { get; set; }
        public decimal TotalSpent { get; set; }
        public List<int> MonthlyBookings { get; set; } = new();
    }
}
