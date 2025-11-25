namespace RentACar.Application.Options
{
    public class StripeOptions
    {
        /// <summary>
        /// Secret API key for server-side Stripe calls.
        /// </summary>
        public string? SecretKey { get; set; }

        /// <summary>
        /// Publishable key for client-side Stripe SDK usage (if needed by the UI).
        /// </summary>
        public string? PublishableKey { get; set; }

        /// <summary>
        /// Default currency code (e.g., "usd").
        /// </summary>
        public string Currency { get; set; } = "usd";
    }
}
