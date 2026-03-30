using System.ComponentModel.DataAnnotations;
using RentACar.Core.Enums;

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

        public decimal? ExtraDriverFeePerDay { get; set; }

        public bool IsAvailable { get; set; }

        public int? CategoryId { get; set; }

        public byte[]? CarImage { get; set; }

        // Optional: To display category name
        public string? CategoryName { get; set; }

        public int SeatsCapacity { get; set; }
        public bool SupportsBabySeat { get; set; }
        public int Doors { get; set; }
        public TransmissionType TransmissionType { get; set; }
        public FuelType FuelType { get; set; }
        public int? LuggageCapacity { get; set; }
        public bool HasInfotainmentScreen { get; set; }
        public bool HasGPS { get; set; }
        public bool HasSunroof { get; set; }
        public bool HasParkingSensors { get; set; }
        public bool HasRearCamera { get; set; }
       
    }
}
