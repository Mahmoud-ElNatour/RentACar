using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class PaymentMethodDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethodName { get; set; } = null!;
    }
}
