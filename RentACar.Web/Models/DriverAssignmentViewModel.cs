using System.Collections.Generic;
using RentACar.Application.DTOs;

namespace RentACar.Web.Models
{
    public class DriverAssignmentViewModel
    {
        public int BookingId { get; set; }
        public bool HasDriver { get; set; }
        public int? DriverId { get; set; }
        public string? DriverName { get; set; }
        public string? DriverEmail { get; set; }
        public string? DriverPhone { get; set; }
        public List<DriverDisplayDto> Drivers { get; set; } = new();
    }
}
