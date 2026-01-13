using System.Collections.Generic;
using System.Threading.Tasks;

namespace RentACar.Core.Repositories
{
    public interface IStripePaymentService
    {
        Task<(string SessionId, string CheckoutUrl)> CreateCheckoutSessionAsync(
            decimal amountUsd,
            string description,
            string successUrl,
            string cancelUrl,
            Dictionary<string, string>? metadata = null);

        Task<bool> IsSessionPaidAsync(string sessionId);
    }
}
