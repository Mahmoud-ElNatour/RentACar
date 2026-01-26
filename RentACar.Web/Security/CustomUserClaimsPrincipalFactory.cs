using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using RentACar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Web.Security
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
    {
        private readonly RentACarDbContext _context;

        public CustomUserClaimsPrincipalFactory(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            RentACarDbContext context)
            : base(userManager, roleManager, optionsAccessor)
        {
            _context = context;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Fetch display name from Customers or Employees table
            string? displayName = null;

            // Try to find in Customers
            var customer = await _context.Customers
                .Where(c => c.aspNetUserId == user.Id)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(customer))
            {
                displayName = customer;
            }
            else
            {
                // Try to find in Employees
                var employee = await _context.Employees
                    .Where(e => e.aspNetUserId == user.Id)
                    .Select(e => e.Name)
                    .FirstOrDefaultAsync();
                
                if (!string.IsNullOrEmpty(employee))
                {
                    displayName = employee;
                }
            }

            // Fallback to username if no name found (though standard Identity adds Name claim usually)
            if (!string.IsNullOrEmpty(displayName))
            {
                identity.AddClaim(new Claim("display_name", displayName));
            }

            return identity;
        }
    }
}
