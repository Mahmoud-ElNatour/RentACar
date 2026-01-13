using System.Threading;
using System.Threading.Tasks;
using RentACar.Application.DTOs;

namespace RentACar.Application.Services
{
    public interface IStripePaymentService
    {
        Task<StripeCheckoutSessionDto> CreateCheckoutSessionAsync(
            StripeCheckoutSessionRequestDto request,
            CancellationToken cancellationToken = default);

        StripeWebhookVerificationResultDto VerifyWebhookSignature(
            string payload,
            string signatureHeader,
            int toleranceSeconds = 300);
    }
}
