using System.Collections.Generic;
using System.Threading.Tasks;

namespace RentACar.Core.Repositories
{
    public interface IDriverAllowedCategoryRepository
    {
        Task<List<int>> GetAllowedCategoryIdsByDriverIdAsync(int driverId);
        Task SetAllowedCategoriesAsync(int driverId, List<int> categoryIds);
        Task<bool> DriverCanDriveCategoryAsync(int driverId, int categoryId);
    }
}
