using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
<<<<<<< HEAD
using RentACar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Ensure LINQ is available
=======
>>>>>>> Mahmoud-V3

namespace RentACar.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Employee")]

    public class ControlPanelController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<ControlPanelController> _logger;
<<<<<<< HEAD
        private readonly RentACarDbContext _context;

        public ControlPanelController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<ControlPanelController> logger, RentACarDbContext context)
=======

        public ControlPanelController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<ControlPanelController> logger)
>>>>>>> Mahmoud-V3
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
<<<<<<< HEAD
            _context = context;
        }

        [HttpGet("SearchUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Ok(new List<object>());
            query = query.ToLower();

            var users = await _context.AspNetUsers
                .GroupJoin(_context.Customers, u => u.Id, c => c.aspNetUserId, (u, c) => new { User = u, Customers = c })
                .SelectMany(x => x.Customers.DefaultIfEmpty(), (x, c) => new { x.User, CustomerName = c != null ? c.Name : null })
                .GroupJoin(_context.Employees, x => x.User.Id, e => e.aspNetUserId, (x, e) => new { x.User, x.CustomerName, Employees = e })
                .SelectMany(x => x.Employees.DefaultIfEmpty(), (x, e) => new { 
                    x.User, 
                    Name = x.CustomerName ?? (e != null ? e.Name : x.User.UserName),
                    x.User.PhoneNumber
                })
                .Where(x => x.User.UserName.Contains(query) || 
                            x.User.Email.Contains(query) || 
                            x.Name.Contains(query) ||
                            (x.PhoneNumber != null && x.PhoneNumber.Contains(query)))
                .Take(10)
                .ToListAsync();

            var result = new List<object>();
            foreach (var item in users)
            {
                var roles = await _userManager.GetRolesAsync(new IdentityUser { Id = item.User.Id, UserName = item.User.UserName }); 
                // Note: GetRolesAsync expects IdentityUser. AspNetUser might not inherit immediately or effectively for UserManager locally unless mapped.
                // Safest to just use currentRole logic if possible, or fetch via userManager if IDs match.
                // Assuming AspNetUser.Id is compatible. 
                // Actually, _userManager.GetRolesAsync might fail if passed a distinct AspNetUser type if not castable. 
                
                // Workaround: We really just want the role name. 
                // Efficiency: We can't easily get role via UserManager from just ID without finding user first.
                // But we are in `_context`. We can Include Roles!
                
                // Let's refetch roles via context for speed?
                // Context AspNetUser has Roles collection.
                
                // Re-querying with Roles included would be better.
                // Updating query above to Include(u => u.Roles) ?
            }
            
            // Refined Loop:
            var resultList = new List<object>();
             // Optimizing: Let's fetch IDs then use UserManager or similar? no, Context is valid.
             // We can just query Roles directly in the LINQ if mapped.
             
            // Re-writing the LINQ to include Roles directly to avoid N+1 and UserManager type issues
             var enrichedUsers = await _context.AspNetUsers
                .Include(u => u.Roles)
                .GroupJoin(_context.Customers, u => u.Id, c => c.aspNetUserId, (u, c) => new { User = u, Customers = c })
                .SelectMany(x => x.Customers.DefaultIfEmpty(), (x, c) => new { x.User, CustomerName = c != null ? c.Name : null })
                .GroupJoin(_context.Employees, x => x.User.Id, e => e.aspNetUserId, (x, e) => new { x.User, x.CustomerName, Employees = e })
                .SelectMany(x => x.Employees.DefaultIfEmpty(), (x, e) => new { 
                    x.User, 
                    Name = x.CustomerName ?? (e != null ? e.Name : x.User.UserName),
                    Roles = x.User.Roles // AspNetUser has Roles collection
                })
                .Where(x => x.User.UserName.Contains(query) || 
                            x.User.Email.Contains(query) || 
                            x.Name.Contains(query) ||
                            (x.User.PhoneNumber != null && x.User.PhoneNumber.Contains(query)))
                .Take(10)
                .ToListAsync();

            foreach(var r in enrichedUsers) {
                 resultList.Add(new {
                    r.User.Id,
                    UserName = r.User.UserName,
                    Name = r.Name,
                    r.User.Email,
                    PhoneNumber = r.User.PhoneNumber ?? "N/A",
                    CurrentRole = r.Roles.FirstOrDefault()?.Name ?? "None"
                 });
            }

            return Ok(resultList);
        }

        [HttpGet("RoleStats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RoleStats()
        {
            var stats = await _context.AspNetRoles
                .Where(r => r.Name == "Admin" || r.Name == "Employee" || r.Name == "Customer")
                .Select(r => new { r.Name, Count = r.Users.Count() })
                .ToDictionaryAsync(x => x.Name, x => x.Count);

            return Ok(new
            {
                AdminCount = stats.ContainsKey("Admin") ? stats["Admin"] : 0,
                EmployeeCount = stats.ContainsKey("Employee") ? stats["Employee"] : 0,
                CustomerCount = stats.ContainsKey("Customer") ? stats["Customer"] : 0
            });
        }

        [HttpGet("GetUsersByRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersByRole(string role, int page = 1, int pageSize = 10)
        {
            if(string.IsNullOrEmpty(role)) return BadRequest("Role required");
            
            // Efficient paging via Context
            var query = _context.AspNetRoles
                .Where(r => r.Name == role)
                .SelectMany(r => r.Users);
                
            var totalItems = await query.CountAsync();
            
            var pagedUsers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .GroupJoin(_context.Customers, u => u.Id, c => c.aspNetUserId, (u, c) => new { User = u, Customers = c })
                .SelectMany(x => x.Customers.DefaultIfEmpty(), (x, c) => new { x.User, CustomerName = c != null ? c.Name : null })
                .GroupJoin(_context.Employees, x => x.User.Id, e => e.aspNetUserId, (x, e) => new { x.User, x.CustomerName, Employees = e })
                .SelectMany(x => x.Employees.DefaultIfEmpty(), (x, e) => new { 
                    x.User, 
                    Name = x.CustomerName ?? (e != null ? e.Name : x.User.UserName)
                })
                .ToListAsync();

            var resultList = pagedUsers.Select(x => new {
                x.User.Id,
                x.User.UserName,
                x.User.Email,
                x.Name,
                PhoneNumber = x.User.PhoneNumber ?? "N/A"
            }).ToList();

            return Ok(new {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                Items = resultList
            });
        }

        [HttpGet("ExportUsersByRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportUsersByRole(string role)
        {
            if(string.IsNullOrEmpty(role)) return BadRequest("Role required");

            var usersQuery = _context.AspNetRoles
                .Where(r => r.Name == role)
                .SelectMany(r => r.Users)
                .GroupJoin(_context.Customers, u => u.Id, c => c.aspNetUserId, (u, c) => new { User = u, Customers = c })
                .SelectMany(x => x.Customers.DefaultIfEmpty(), (x, c) => new { x.User, CustomerName = c != null ? c.Name : null })
                .GroupJoin(_context.Employees, x => x.User.Id, e => e.aspNetUserId, (x, e) => new { x.User, x.CustomerName, Employees = e })
                .SelectMany(x => x.Employees.DefaultIfEmpty(), (x, e) => new { 
                    x.User, 
                    Name = x.CustomerName ?? (e != null ? e.Name : x.User.UserName)
                });

            var details = await usersQuery.ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Id,Name,UserName,Email,PhoneNumber,Role");
            
            foreach(var item in details)
            {
                sb.AppendLine($"{item.User.Id},{item.Name},{item.User.UserName},{item.User.Email},{item.User.PhoneNumber},{role}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"{role}_Users_Export_{System.DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        [HttpGet("~/ControlPanel")]
        [Authorize(Roles = "Admin,Employee")]
=======
        }

        [HttpGet("~/ControlPanel")]
>>>>>>> Mahmoud-V3
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("~/ControlPanel/ChangeRole")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult ChangeRole()
        {
            return View();
        }


<<<<<<< HEAD

=======
>>>>>>> Mahmoud-V3
        [HttpPost("ChangeRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDTO model)
        {
            _logger.LogInformation("Changing role for {User}", model.UserName);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                var createResult = await _roleManager.CreateAsync(new IdentityRole(model.Role));
                if (!createResult.Succeeded)
                {
                    return BadRequest(new { message = "Could not create role." });
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return BadRequest(new { message = "Could not remove existing roles." });
            }

            var addResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addResult.Succeeded)
            {
                return BadRequest(new { message = "Could not assign role." });
            }

            return Ok(new { message = "Role updated successfully." });
        }
    }
}
