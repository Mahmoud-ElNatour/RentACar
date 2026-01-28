using System.Collections.Generic;
using RentACar.Application.DTOs;

namespace RentACar.Web.Models
{
    public class HomeIndexViewModel
    {
        public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public int TotalCars { get; set; }
    }
}
