using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RentACar.Core.Entities;
using RentACar.Core.Repositories.Base;

namespace RentACar.Core.Repositories
{
    public interface ICreditCardRepository : IRepository<CreditCard>
    {
        Task<CreditCard?> GetByCardNumberAsync(string cardNumber);
        Task<List<CreditCard>> GetCustomerCreditCardsAsync(int userId);
        Task AddCustomerCreditCardAsync(CustomerCreditCard customerCreditCard);
        Task RemoveCustomerCreditCardAsync(int userId, int creditCardId);
        Task RemoveCreditCardAsync(int userId, int creditCardId);
    }
}
