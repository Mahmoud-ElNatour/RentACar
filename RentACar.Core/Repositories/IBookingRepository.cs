using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RentACar.Core.Entities;
using RentACar.Core.Repositories.Base;

namespace RentACar.Core.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<Booking> GetBookingByIdAsync(int id);
        Task<List<Booking>> GetBookingsByEmployeeIdAsync(int employeeId);
        Task<List<Booking>> GetBookingsByCustomerIdAsync(int customerId);
        Task<List<Booking>> GetBookingsByCarIdAsync(int carId);
        Task<List<Booking>> GetBookingsBetweenDatesAsync(DateOnly startDate, DateOnly endDate);
    }
}
