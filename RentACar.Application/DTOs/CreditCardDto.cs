using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class CreditCardDto
    {
        public int CreditCardId { get; set; }

        [Required]
        [StringLength(50)]
        public string CardNumber { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string CardHolderName { get; set; } = null!;

        [Required]
        public DateOnly ExpiryDate { get; set; }

        [Required]
        [StringLength(10)]
        public string Cvv { get; set; } = null!;
    }
}