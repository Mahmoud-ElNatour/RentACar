using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class CarListDto
    {
        public int CarId { get; set; }

        public string PlateNumber { get; set; } = null!;

        public string ModelName { get; set; } = null!;

        public int ModelYear { get; set; }

        public string? Color { get; set; }

        public decimal? PricePerDay { get; set; }

        public bool IsAvailable { get; set; }

        public int? CategoryId { get; set; }

        public string? CategoryName { get; set; }
    }
}
