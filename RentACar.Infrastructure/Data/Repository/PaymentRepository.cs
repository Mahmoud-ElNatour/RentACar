using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data.Repository.Base;

namespace RentACar.Infrastructure.Data.Repository
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        private readonly RentACarDbContext _dbContext;

        public PaymentRepository(RentACarDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Payment>> GetPaymentsByBookingIdAsync(int bookingId)
        {
            return await _dbContext.Payments
                                   .Where(p => p.BookingId == bookingId)
                                   .ToListAsync();
        }

        public async Task<List<Payment>> GetAllWithDetailsAsync()
        {
            return await _dbContext.Payments
                                   .Include(p => p.Booking)
                                       .ThenInclude(b => b.Customer)
                                           .ThenInclude(c => c.User)
                                   .Include(p => p.Booking)
                                       .ThenInclude(b => b.Car)
                                   .Include(p => p.Booking)
                                       .ThenInclude(b => b.Promocode)
                                   .AsNoTracking()
                                   .ToListAsync();
        }

        public async Task<Payment?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbContext.Payments
                                   .Include(p => p.Booking)
                                       .ThenInclude(b => b.Customer)
                                           .ThenInclude(c => c.User)
                                   .Include(p => p.Booking)
                                       .ThenInclude(b => b.Car)
                                   .Include(p => p.Booking)
                                       .ThenInclude(b => b.Promocode)
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync(p => p.PaymentId == id);
        }
    }
}
