﻿using Microsoft.EntityFrameworkCore;
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

        public async Task DeleteAsync(int id)
        {
            var payment = await _dbContext.Payments.FindAsync(id);
            if (payment != null)
            {
                _dbContext.Payments.Remove(payment);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
