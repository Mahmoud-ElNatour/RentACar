using System;

namespace RentACar.Application.DTOs.Support
{
    public class SupportConversationListDto
    {
        public int ConversationId { get; set; }
        public string Subject { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public int? BookingId { get; set; }
        public string CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? AssignedEmployeeId { get; set; }
        public string? AssignedEmployeeName { get; set; }
        public string LastMessageSnippet { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
