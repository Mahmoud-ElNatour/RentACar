using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Core.Entities;

namespace RentACar.Core.Repositories;

public interface IDriverRepository : IRepository<Driver>
{
    Task<Driver?> GetByAspNetUserIdAsync(string aspNetUserId);
    Task<List<Driver>> GetActiveDriversAsync();
}
