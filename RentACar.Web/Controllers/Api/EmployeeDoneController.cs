using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Core.Entities;
using RentACar.Infrastructure.Data;
using RentACar.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Employee,Manager")]
    public class EmployeeDoneController : ControllerBase
    {
        private readonly RentACarDbContext _context;
        private readonly EmployeeManager _employeeManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public EmployeeDoneController(
            RentACarDbContext context,
            EmployeeManager employeeManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper)
        {
            _context = context;
            _employeeManager = employeeManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> Get(
            [FromQuery] string? search,
            [FromQuery] bool? active,
            [FromQuery] string? role)
        {
            var query = _context.Employees
                .Include(e => e.User)
                .Include(e => e.Driver)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(e =>
                    e.Name.ToLower().Contains(lowerSearch) ||
                    e.User.Email.ToLower().Contains(lowerSearch) ||
                    (e.Driver != null && e.Driver.DriverCode.ToLower().Contains(lowerSearch)));
            }

            if (active.HasValue)
            {
                query = query.Where(e => e.IsActive == active.Value);
            }

            var employees = await query.ToListAsync();
            var results = new List<EmployeeDto>();

            foreach (var employee in employees)
            {
                var dto = _mapper.Map<EmployeeDto>(employee);
                var user = await _userManager.FindByIdAsync(employee.aspNetUserId);
                dto.Roles = user == null ? new List<string>() : (await _userManager.GetRolesAsync(user)).ToList();

                if (!string.IsNullOrWhiteSpace(role) && !dto.Roles.Contains(role))
                {
                    continue;
                }

                results.Add(dto);
            }

            return Ok(results);
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> Create([FromBody] EmployeeDoneUpsertDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Password is required." });
            }

            try
            {
                var created = await _employeeManager.CreateEmployee(new EmployeeCreateDTO
                {
                    Name = dto.Name,
                    Salary = dto.Salary,
                    Address = dto.Address,
                    IsActive = dto.IsActive,
                    Email = dto.Email,
                    Password = dto.Password,
                    PhoneNumber = dto.PhoneNumber
                });

                if (created == null)
                {
                    return StatusCode(500, new { message = "Unable to create employee." });
                }

                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user != null)
                {
                    await UpdateUserRoles(user, dto.Roles);
                }

                var employee = await _context.Employees
                    .Include(e => e.Driver)
                    .FirstOrDefaultAsync(e => e.EmployeeId == created.EmployeeId);

                if (employee != null && ShouldHaveDriver(dto))
                {
                    await UpsertDriverAsync(employee, dto);
                }

                var resultDto = employee == null ? created : _mapper.Map<EmployeeDto>(employee);
                if (user != null)
                {
                    resultDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                }

                return Ok(resultDto);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeDoneUpsertDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0)
            {
                return BadRequest("Invalid employee ID.");
            }

            var employee = await _context.Employees
                .Include(e => e.Driver)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            var updateDto = new EmployeeDto
            {
                EmployeeId = id,
                Name = dto.Name,
                Salary = dto.Salary,
                Address = dto.Address,
                IsActive = dto.IsActive,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                username = dto.Email,
                aspNetUserId = employee.aspNetUserId
            };

            try
            {
                await _employeeManager.UpdateEmployee(updateDto);

                var user = await _userManager.FindByIdAsync(employee.aspNetUserId);
                if (user != null)
                {
                    await UpdateUserRoles(user, dto.Roles);
                }

                if (ShouldHaveDriver(dto))
                {
                    await UpsertDriverAsync(employee, dto);
                }
                else if (employee.Driver != null)
                {
                    employee.Driver.IsActive = false;
                    employee.Driver.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _employeeManager.DeleteEmployee(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/reset-password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(employee.aspNetUserId);
            if (user == null)
            {
                return NotFound();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, "E@e123456");

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to reset password." });
            }

            return NoContent();
        }

        private async Task UpdateUserRoles(IdentityUser user, IEnumerable<string> roles)
        {
            var roleList = roles?.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList() ?? new List<string>();
            if (roleList.Count == 0)
            {
                roleList.Add("Employee");
            }

            foreach (var role in roleList)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException("Could not remove existing roles.");
            }

            var addResult = await _userManager.AddToRolesAsync(user, roleList);
            if (!addResult.Succeeded)
            {
                throw new InvalidOperationException("Could not assign roles.");
            }
        }

        private static bool ShouldHaveDriver(EmployeeDoneUpsertDto dto)
        {
            return dto.Roles != null && dto.Roles.Any(r => r.Equals("Driver", StringComparison.OrdinalIgnoreCase));
        }

        private async Task UpsertDriverAsync(Employee employee, EmployeeDoneUpsertDto dto)
        {
            var driver = employee.Driver;
            if (driver == null)
            {
                driver = new Driver
                {
                    EmployeeId = employee.EmployeeId,
                    AspNetUserId = employee.aspNetUserId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Drivers.Add(driver);
            }

            driver.DriverCode = string.IsNullOrWhiteSpace(dto.DriverCode)
                ? driver.DriverCode ?? $"DRV-{employee.EmployeeId}"
                : dto.DriverCode;
            driver.FullName = string.IsNullOrWhiteSpace(dto.DriverFullName) ? employee.Name : dto.DriverFullName!;
            driver.Phone = string.IsNullOrWhiteSpace(dto.DriverPhone) ? dto.PhoneNumber : dto.DriverPhone;
            driver.Email = string.IsNullOrWhiteSpace(dto.DriverEmail) ? dto.Email : dto.DriverEmail!;
            driver.Rating = dto.DriverRating;
            driver.LicenseNumber = dto.DriverLicenseNumber;
            driver.LicenseExpiry = dto.DriverLicenseExpiry;
            driver.Languages = dto.DriverLanguages;
            driver.Notes = dto.DriverNotes;
            driver.IsActive = dto.DriverIsActive;
            driver.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
