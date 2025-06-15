using System;

namespace RentACar.Application.DTOs
{
    public class CreditCardDisplayDto
    {
        public int CreditCardId { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public DateOnly ExpiryDate { get; set; }
        public string Cvv { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int CustomerId { get; set; }
    }
}
