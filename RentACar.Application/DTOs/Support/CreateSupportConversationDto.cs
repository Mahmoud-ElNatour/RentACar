using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs.Support
{
    public class CreateSupportConversationDto
    {
        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        [Required]
        [StringLength(150)]
        public string Subject { get; set; }

        [Required]
        public string InitialMessage { get; set; }

        public int? BookingId { get; set; }
    }
}
