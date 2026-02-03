using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.DTOs;
using RentACar.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee,Manager")]
    public class EmployeeDoneController : Controller
    {
        private readonly RentACarDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public EmployeeDoneController(
            RentACarDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        [HttpGet("~/EmployeeDone")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Views/EmployeeDone/Index.cshtml");
        }

        [HttpGet("~/EmployeeDone/Add")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> AddForm()
        {
            ViewBag.Roles = await GetRoleNamesAsync();
            return PartialView("~/Views/EmployeeDone/_EmployeeFormPartial.cshtml",
                new EmployeeDto { IsActive = true, Roles = new List<string> { "Employee" } });
        }

        [HttpGet("~/EmployeeDone/Edit/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditForm(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.Driver)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<EmployeeDto>(employee);
            var user = await _userManager.FindByIdAsync(employee.aspNetUserId);
            dto.Roles = user == null ? new List<string>() : (await _userManager.GetRolesAsync(user)).ToList();

            ViewBag.Roles = await GetRoleNamesAsync();
            return PartialView("~/Views/EmployeeDone/_EmployeeFormPartial.cshtml", dto);
        }

        [HttpGet("~/EmployeeDone/Delete/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            var dto = new EmployeeDto
            {
                EmployeeId = employee.EmployeeId,
                Name = employee.Name
            };

            return PartialView("~/Views/EmployeeDone/_DeleteEmployeePartial.cshtml", dto);
        }

        [HttpGet("~/EmployeeDone/ChangeRole")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Roles = "Admin")]
        public IActionResult ChangeRole()
        {
            return View("~/Views/EmployeeDone/ChangeRole.cshtml", new ChangeRoleDTO());
        }

        private async Task<IEnumerable<string>> GetRoleNamesAsync()
        {
            var roles = await _roleManager.Roles
                .Select(r => r.Name)
                .Where(name => name != null)
                .ToListAsync();

            var defaults = new[] { "Admin", "Employee", "Driver", "Manager" };
            foreach (var role in defaults)
            {
                if (!roles.Contains(role))
                {
                    roles.Add(role);
                }
            }

            return roles.OrderBy(r => r).ToList();
        }
    }
}
