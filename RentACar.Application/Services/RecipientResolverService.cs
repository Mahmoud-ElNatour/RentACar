using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.Managers;
using RentACar.Infrastructure.Data;

namespace RentACar.Application.Services
{
    public class RecipientResolverService
    {
        private readonly RentACarDbContext _context;
        private readonly DistributionListManager _distributionListManager;

        public RecipientResolverService(
            RentACarDbContext context,
            DistributionListManager distributionListManager)
        {
            _context = context;
            _distributionListManager = distributionListManager;
        }

        private async Task<List<string>> GetEmailsFromListOrFallbackAsync(int? listId, Func<Task<List<string>>> fallbackLogic)
        {
            if (listId.HasValue)
            {
                var list = await _distributionListManager.GetListByIdAsync(listId.Value);
                if (list != null && list.IsActive)
                {
                    // Return active members from the list
                    return list.Members
                        .Where(m => m.IsActive)
                        .Select(m => m.Email)
                        .Distinct()
                        .ToList();
                }
            }

            // Fallback
            return await fallbackLogic();
        }

        public async Task<List<string>> GetRecipientsForCarUpdateAsync()
        {
            // Default: All Active Employees
            return await _context.Employees
                .Where(e => e.User.Email != null)
                .Select(e => e.User.Email)
                .ToListAsync();
        }

        public async Task<List<string>> GetRecipientsForCategoryUpdateAsync()
        {
            return await _context.Employees
                .Where(e => e.User.Email != null)
                .Select(e => e.User.Email)
                .ToListAsync();
        }

        public async Task<List<string>> GetRecipientsForPromocodeUpdateAsync()
        {
            return await _context.Employees
                .Where(e => e.User.Email != null)
                .Select(e => e.User.Email)
                .ToListAsync();
        }

        public async Task<List<string>> GetRecipientsForPromoExpiryAsync()
        {
            // Default: All Active Employees
            return await _context.Employees
                .Where(e => e.User.Email != null)
                .Select(e => e.User.Email)
                .ToListAsync();
        }

        // Generic method for arbitrary lists
        public async Task<List<string>> ResolveFromListIdAsync(int listId)
        {
            return await GetEmailsFromListOrFallbackAsync(listId, async () => new List<string>());
        }
    }
}
