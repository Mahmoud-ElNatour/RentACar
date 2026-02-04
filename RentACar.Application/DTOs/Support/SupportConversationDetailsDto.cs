using System;
using System.Collections.Generic;

namespace RentACar.Application.DTOs.Support
{
    public class SupportConversationDetailsDto
    {
        public int ConversationId { get; set; }
        public string Subject { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public int? BookingId { get; set; }
        public int RealCustomerId { get; set; }
        public string CustomerId { get; set; } // This is aspNetUserId
        public string CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? AssignedEmployeeId { get; set; }
        public string? AssignedEmployeeName { get; set; }
        public SupportBookingDto? ActiveBooking { get; set; }
        public List<SupportConversationListDto> MyActiveConversations { get; set; } = new List<SupportConversationListDto>();
        public List<SupportMessageDto> Messages { get; set; } = new List<SupportMessageDto>();
    }

    public class SupportBookingDto
    {
        public int BookingId { get; set; }
        public string CarName { get; set; }
        public string PlateNumber { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Status { get; set; }
    }
}
