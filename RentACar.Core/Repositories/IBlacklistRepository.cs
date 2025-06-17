using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RentACar.Core.Entities;
using RentACar.Core.Repositories.Base;

namespace RentACar.Core.Repositories
{
    public interface IBlacklistRepository : IRepository<BlackList>
    {
        Task<BlackList?> GetByUserIdAsync(String userId);
    }
    
}
