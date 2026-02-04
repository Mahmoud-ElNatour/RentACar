using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs.Support
{
    public class UpdateSupportConversationStatusDto
    {
        [Required]
        public int ConversationId { get; set; }

        [Required]
        public string NewStatus { get; set; }
    }
}
