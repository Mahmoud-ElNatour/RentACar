using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs.Support
{
    public class AssignSupportConversationDto
    {
        [Required]
        public int ConversationId { get; set; }

        [Required]
        public string AssignedEmployeeId { get; set; }

        public string? Note { get; set; }
    }
}
