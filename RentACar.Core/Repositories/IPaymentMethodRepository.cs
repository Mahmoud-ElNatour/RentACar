using RentACar.Core.Entities;
using RentACar.Core.Repositories.Base;

namespace RentACar.Core.Repositories
{
    public interface IPaymentMethodRepository : IRepository<PaymentMethod>
    {
        Task DeleteAsync(int id);
        Task<PaymentMethod?> GetByNameAsync(string name);
    }
}
