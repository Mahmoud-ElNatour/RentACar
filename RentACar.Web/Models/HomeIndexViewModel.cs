using System.Collections.Generic;
using RentACar.Application.DTOs;

namespace RentACar.Web.Models
{
    public class HomeIndexViewModel
    {
        public string? UserRole { get; set; }
        public int AvailableCars { get; set; }
        public int TotalCustomers { get; set; }
        public List<CategoryDto> Categories { get; set; } = new();
    }
}
