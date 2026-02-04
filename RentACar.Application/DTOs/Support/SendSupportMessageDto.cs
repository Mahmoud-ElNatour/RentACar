using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs.Support
{
    public class SendSupportMessageDto
    {
        [Required]
        public int ConversationId { get; set; }

        [Required]
        public string MessageText { get; set; }

        public string? AttachmentUrl { get; set; }
    }
}
