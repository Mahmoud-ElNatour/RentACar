using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repository
{
    public class PaymentMethodRepository : Repository<PaymentMethod>, IPaymentMethodRepository
    {
        private readonly RentACarDbContext _dbContext;

        public PaymentMethodRepository(RentACarDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task DeleteAsync(int id)
        {
            var method = await _dbContext.PaymentMethods.FindAsync(id);
            if (method != null)
            {
                _dbContext.PaymentMethods.Remove(method);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<PaymentMethod?> GetByNameAsync(string name)
        {
            return await _dbContext.PaymentMethods.FirstOrDefaultAsync(p => p.PaymentMethodName == name);
        }
    }
}
