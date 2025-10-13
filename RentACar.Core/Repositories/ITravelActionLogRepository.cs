using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;

namespace RentACar.Core.Repositories;

public interface ITravelActionLogRepository
{
    Task<TravelActionLog> AddAsync(TravelActionLog log);
    Task<List<TravelActionLog>> GetRecentAsync(int limit = 100);
    Task<List<TravelActionLog>> GetByCustomerUsernameAsync(string customerUsername, int limit = 100);
    Task<List<TravelActionLog>> GetByDateRangeAsync(DateTime fromUtc, DateTime toUtc, int limit = 200);
}
