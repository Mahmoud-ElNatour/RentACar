using RentACar.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RentACar.Core.Repositories.Base;

namespace RentACar.Core.Repositories
{
   public  interface IPaymentRepository : IRepository<Payment>
    {
        Task<List<Payment>> GetPaymentsByBookingIdAsync(int bookingId);

    }

}
