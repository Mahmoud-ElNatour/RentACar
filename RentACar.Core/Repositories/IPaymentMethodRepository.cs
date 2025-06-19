using RentACar.Core.Entities;
using RentACar.Core.Repositories.Base;
using System.Threading.Tasks;

namespace RentACar.Core.Repositories
{
    public interface IPaymentMethodRepository : IRepository<PaymentMethod>
    {
        /// <summary>
        /// Deletes a payment method by its ID.
        /// </summary>
        /// <param name="id">The ID of the payment method to delete.</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Retrieves a payment method by its name.
        /// </summary>
        /// <param name="name">The name of the payment method.</param>
        /// <returns>The matching PaymentMethod entity, or null if not found.</returns>
        Task<PaymentMethod?> GetByNameAsync(string name);
    }
}
