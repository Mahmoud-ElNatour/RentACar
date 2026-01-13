
using Stripe;
using Stripe.Checkout;
using RentACar.Core.Repositories.Base;
using RentACar.Core.Repositories;
using Microsoft.Extensions.Configuration;

namespace RentACar.Infrastructure.Repository.Base
{
    public class StripePaymentService : IStripePaymentService
    {
        public StripePaymentService(IConfiguration config)
        {
            StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        }

        public async Task<(string SessionId, string CheckoutUrl)> CreateCheckoutSessionAsync(
            decimal amountUsd,
            string description,
            string successUrl,
            string cancelUrl,
            Dictionary<string, string>? metadata = null)
        {
            var amountCents = (long)Math.Round(amountUsd * 100m);

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = amountCents,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = description
                            }
                        }
                    }
                },
                Metadata = metadata
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return (session.Id, session.Url);
        }

        public async Task<bool> IsSessionPaidAsync(string sessionId)
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);

            // payment_status is "paid" when successful
            return string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase);
        }
    }
}
