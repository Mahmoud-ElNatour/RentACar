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

<<<<<<< HEAD
        // Only include the base64 string if it's small, OR preferably exclude it entirely 
        // and let the frontend load via /api/Car/Image/{id}
        // Ideally we ONLY serve metadata here.
        // public byte[]? CarImage { get; set; } // EXCLUDED for performance

=======
>>>>>>> Mahmoud-V3
        public string? CategoryName { get; set; }
    }
}
