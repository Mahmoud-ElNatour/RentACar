using RentACar.Core.Entities;
using RentACar.Core.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentACar.Core.Repositories
{
    public interface IPromocodeRepository : IRepository<Promocode>
    {
        Task<Promocode?> GetByNameAsync(string name);
        Task<List<Promocode>> GetActiveAsync();
        Task<Promocode?> GetByCodeAsync(string code);



    }
}
