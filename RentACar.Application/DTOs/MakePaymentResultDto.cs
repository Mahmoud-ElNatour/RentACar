namespace RentACar.Application.DTOs
{
    public class MakePaymentResultDto
    {
        public PaymentDto Payment { get; set; } = null!;

        public bool RequiresRedirect { get; set; }

        public string? RedirectUrl { get; set; }
    }
}
