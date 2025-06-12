using System.ComponentModel.DataAnnotations;
using RentACar.Application.DTOs;

namespace RentACar.Application.DTOs
{
    public class CustomerCreditCardDto
    {
        public int CustomerCreditCardId { get; set; }

        [Required]
        [StringLength(450)]
        public int UserId { get; set; } = 0!;

        [Required]
        public int CreditCardId { get; set; }

        // Optionally, you might include navigation properties if needed for transfer
        public CreditCardDto CreditCard { get; set; }
    }
}