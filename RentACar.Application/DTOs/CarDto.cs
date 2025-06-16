using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class CarDto
    {
        public int CarId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PlateNumber { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string ModelName { get; set; } = null!;

        public int ModelYear { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        public decimal? PricePerDay { get; set; }

        public bool IsAvailable { get; set; }

        public int? CategoryId { get; set; }

        public byte[]? CarImage { get; set; }

        // Optional: To display category name
        public string? CategoryName { get; set; }
       
    }
}