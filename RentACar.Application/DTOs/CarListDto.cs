using System.ComponentModel.DataAnnotations;
using RentACar.Core.Enums;

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

        // Only include the base64 string if it's small, OR preferably exclude it entirely 
        // and let the frontend load via /api/Car/Image/{id}
        // Ideally we ONLY serve metadata here.
        // public byte[]? CarImage { get; set; } // EXCLUDED for performance
        public string? CategoryName { get; set; }

        public int SeatsCapacity { get; set; }
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
