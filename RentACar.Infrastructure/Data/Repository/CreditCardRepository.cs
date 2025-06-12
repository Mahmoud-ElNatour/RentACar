using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repository
{
    public class CreditCardRepository : Repository<CreditCard>, ICreditCardRepository
    {
        private readonly RentACarDbContext _dbContext; // Adjust DbContext type if needed

        public CreditCardRepository(RentACarDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CreditCard?> GetByCardNumberAsync(string cardNumber)
        {
            return await _dbContext.CreditCards.FirstOrDefaultAsync(cc => cc.CardNumber == cardNumber);
        }

        public async Task<List<CreditCard>> GetCustomerCreditCardsAsync(int userId)
        {
            return await _dbContext.CustomerCreditCards
                .Where(ccc => ccc.UserId == userId)
                .Select(ccc => ccc.CreditCard)
                .ToListAsync();
        }

        public async Task AddCustomerCreditCardAsync(CustomerCreditCard customerCreditCard)
        {
            await _dbContext.CustomerCreditCards.AddAsync(customerCreditCard);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveCustomerCreditCardAsync(int userId, int creditCardId)
        {
            var customerCreditCard = await _dbContext.CustomerCreditCards
                .FirstOrDefaultAsync(ccc => ccc.UserId == userId && ccc.CreditCardId == creditCardId);
            if (customerCreditCard != null)
            {
                _dbContext.CustomerCreditCards.Remove(customerCreditCard);
                await _dbContext.SaveChangesAsync();
            }
        }

        public Task RemoveCreditCardAsync(int userId, int creditCardId)
        {
            var customerCreditCard = _dbContext.CustomerCreditCards
                .FirstOrDefault(ccc => ccc.UserId == userId && ccc.CreditCardId == creditCardId);
            if (customerCreditCard != null)
            {
                _dbContext.CustomerCreditCards.Remove(customerCreditCard);
                return _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException("Customer credit card not found.");
            }
        }
    }


}

