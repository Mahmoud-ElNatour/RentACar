using System;

namespace RentACar.Application.DTOs.Support
{
    public class SupportMessageDto
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public string SenderUserId { get; set; }
        public string SenderDisplayName { get; set; }
        public string SenderRole { get; set; }
        public string MessageText { get; set; }
        public string? AttachmentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsInternalNote { get; set; }
    }
}
