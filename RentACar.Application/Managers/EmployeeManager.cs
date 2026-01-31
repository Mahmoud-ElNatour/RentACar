using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using Microsoft.Extensions.Logging;
<<<<<<< HEAD
using Microsoft.EntityFrameworkCore;
=======
>>>>>>> Mahmoud-V3

namespace RentACar.Application.Managers
{
    public class EmployeeManager
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmployeeRepository _employeeRepository;
<<<<<<< HEAD
=======
        private readonly IDriverRepository _driverRepository;
>>>>>>> Mahmoud-V3
        private readonly IMapper _mapper;
        private readonly CustomerManager _customerManager; // To access CustomerManager methods
        private readonly ILogger<EmployeeManager> _logger;
        private readonly IBookingRepository _bookingRepository;
        private readonly AuditLogManager _auditLogManager;
<<<<<<< HEAD
        private readonly EmailManager _emailManager;

        public EmployeeManager(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IEmployeeRepository employeeRepository, IBookingRepository bookingRepository, IMapper mapper, CustomerManager customerManager, ILogger<EmployeeManager> logger, AuditLogManager auditLogManager, EmailManager emailManager)
=======

        public EmployeeManager(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IEmployeeRepository employeeRepository, IDriverRepository driverRepository, IBookingRepository bookingRepository, IMapper mapper, CustomerManager customerManager, ILogger<EmployeeManager> logger, AuditLogManager auditLogManager)
>>>>>>> Mahmoud-V3
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _employeeRepository = employeeRepository;
<<<<<<< HEAD
=======
            _driverRepository = driverRepository;
>>>>>>> Mahmoud-V3
            _mapper = mapper;
            _customerManager = customerManager;
            _logger = logger;
            _bookingRepository = bookingRepository;
            _auditLogManager = auditLogManager;
<<<<<<< HEAD
            _emailManager = emailManager;
=======
>>>>>>> Mahmoud-V3
        }

        public async Task<EmployeeDto?> CreateEmployee(EmployeeCreateDTO createDto)
        {
            _logger.LogInformation("Creating employee for {Email}", createDto.Email);
            var username = createDto.Email;

            var existingByUsername = await _userManager.FindByNameAsync(username);
            if (existingByUsername != null)
            {
                throw new InvalidOperationException("Username is already in use by another user.");
            }

            var existingByEmail = await _userManager.FindByEmailAsync(createDto.Email);
            if (existingByEmail != null)
            {
                throw new InvalidOperationException("Email address is already registered.");
            }

            var user = new IdentityUser
            {
                UserName = username,
                Email = createDto.Email,
                PhoneNumber = createDto.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, createDto.Password);

            if (result.Succeeded)
            {
<<<<<<< HEAD
                if (!await _roleManager.RoleExistsAsync("Employee"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Employee"));
                }

                await _userManager.AddToRoleAsync(user, "Employee");
=======
                var roles = createDto.Roles?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();
                if (!roles.Any())
                {
                    roles.Add("Employee");
                }

                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                await _userManager.AddToRolesAsync(user, roles);
>>>>>>> Mahmoud-V3

                var employee = _mapper.Map<Employee>(createDto);
                employee.IsActive = createDto.IsActive;
                employee.aspNetUserId = user.Id;
                await _employeeRepository.AddAsync(employee);
                _logger.LogInformation("Employee created with id {Id}", employee.EmployeeId);

<<<<<<< HEAD
=======
                if (roles.Any(r => string.Equals(r, "Driver", StringComparison.OrdinalIgnoreCase)))
                {
                    var driver = await CreateOrUpdateDriverAsync(employee, createDto);
                    _logger.LogInformation("Driver extension created for employee {Id}", employee.EmployeeId);
                }

>>>>>>> Mahmoud-V3
                await _auditLogManager.LogAsync("Create", "Employee", employee.EmployeeId.ToString(), $"Created new employee: {employee.Name} ({createDto.Email})");

                return _mapper.Map<EmployeeDto>(employee);
            }
            var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                errorMessage = "Unable to create employee account.";
            }

            _logger.LogWarning("Failed to create employee for {Email}: {Message}", createDto.Email, errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
<<<<<<< HEAD
        public async Task<bool> ResetPassword(int EmployeeId)
=======
        public async Task<bool> ResetPassword(int EmployeeId, string newPassword)
>>>>>>> Mahmoud-V3
        {
            var employee = await _employeeRepository.GetByIdAsync(EmployeeId);
            if (employee == null) return false;
            var user = await _userManager.FindByIdAsync(employee.aspNetUserId);
            if (user == null) return false;
<<<<<<< HEAD
            
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var newPassword = $"RentCar{new Random().Next(100000, 999999)}!";
            
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            
             if (result.Succeeded)
            {
                 await _emailManager.SendAdminResetPasswordEmail(user.Email, newPassword, employee.Name);
                 await _auditLogManager.LogEventAsync("Employee.PasswordReset", "Employee", EmployeeId.ToString(), "Admin reset employee password", null, "Success");
            }
            
=======
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
>>>>>>> Mahmoud-V3
            return result.Succeeded;
        }
        public async Task<EmployeeDto?> GetEmployeeById(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
<<<<<<< HEAD
            return _mapper.Map<EmployeeDto>(employee);
=======
            if (employee == null)
            {
                return null;
            }

            var dto = _mapper.Map<EmployeeDto>(employee);
            await EnrichEmployeeDtoAsync(dto, employee);
            return dto;
>>>>>>> Mahmoud-V3
        }

        public async Task<List<EmployeeDto>> GetAllEmployees()
        {
            var employees = await _employeeRepository.GetAllAsync();
<<<<<<< HEAD
            return _mapper.Map<List<EmployeeDto>>(employees);
        }

        public async Task<EmployeeDto?> GetEmployeeByAspNetUserId(string aspNetUserId)
        {
            var employee = await _employeeRepository.GetByIdAsync(aspNetUserId);
            return _mapper.Map<EmployeeDto>(employee);
=======
            var dtos = _mapper.Map<List<EmployeeDto>>(employees);
            for (var i = 0; i < employees.Count; i++)
            {
                await EnrichEmployeeDtoAsync(dtos[i], employees[i]);
            }
            return dtos;
>>>>>>> Mahmoud-V3
        }

        public async Task UpdateEmployee(EmployeeDto employeeDto)
        {
<<<<<<< HEAD
            _logger.LogInformation("Updating employee {Id}", employeeDto.EmployeeId);
            
            if (employeeDto.EmployeeId <= 0)
            {
                _logger.LogWarning("UpdateEmployee called with invalid EmployeeId: {Id}. Attempting to resolve via aspNetUserId.", employeeDto.EmployeeId);
                if (!string.IsNullOrEmpty(employeeDto.aspNetUserId))
                {
                    var resolved = await _employeeRepository.GetByIdAsync(employeeDto.aspNetUserId);
                    if (resolved != null) employeeDto.EmployeeId = resolved.EmployeeId;
                }
            }
            
            var employeeEntity = await _employeeRepository.GetByIdAsync(employeeDto.EmployeeId);
            if (employeeEntity == null)
            {
                _logger.LogError("Employee with EmployeeId {Id} not found. Update aborted.", employeeDto.EmployeeId);
                throw new KeyNotFoundException($"Employee with ID {employeeDto.EmployeeId} not found.");
            }
            
            var user = await _userManager.FindByIdAsync(employeeEntity.aspNetUserId);
            if (user != null)
            {
                employeeDto.username ??= employeeDto.Email;
                var existingByEmail = await _userManager.FindByEmailAsync(employeeDto.Email);
                if (existingByEmail != null && existingByEmail.Id != user.Id)
                {
                    throw new InvalidOperationException("Email address is already registered to another user.");
                }

                var existingByUsername = await _userManager.FindByNameAsync(employeeDto.username);
                if (existingByUsername != null && existingByUsername.Id != user.Id)
                {
                    throw new InvalidOperationException("Username is already in use by another user.");
                }

                user.Email = employeeDto.Email;
                user.UserName = employeeDto.username;
                user.PhoneNumber = employeeDto.PhoneNumber;
                await _userManager.UpdateAsync(user);
            }

            var oldActive = employeeEntity.IsActive;
            
            employeeEntity.Name = employeeDto.Name;
            employeeEntity.Salary = employeeDto.Salary;
            employeeEntity.Address = employeeDto.Address;
            employeeEntity.IsActive = employeeDto.IsActive;

            await _employeeRepository.UpdateAsync(employeeEntity);
            _logger.LogInformation("Employee {Id} updated successfully in repository.", employeeDto.EmployeeId);
            await _auditLogManager.LogAsync("Update", "Employee", employeeDto.EmployeeId.ToString(), $"Updated employee profile: {employeeDto.Name}");
            
            if (oldActive != employeeDto.IsActive)
            {
                string status = employeeEntity.IsActive ? "Activated" : "Deactivated";
                string reason = employeeEntity.IsActive ? "Account activated." : "Account deactivated.";
                await _emailManager.SendAccountStatusEmail(user.Email, employeeEntity.Name, status, reason);
            }
=======
            var employeeEntity = await _employeeRepository.GetByIdAsync(employeeDto.EmployeeId);
            if (employeeEntity != null)
            {
                var user = await _userManager.FindByIdAsync(employeeEntity.aspNetUserId);
                if (user != null)
                {
                    employeeDto.username ??= employeeDto.Email;
                    var existingByEmail = await _userManager.FindByEmailAsync(employeeDto.Email);
                    if (existingByEmail != null && existingByEmail.Id != user.Id)
                    {
                        throw new InvalidOperationException("Email address is already registered to another user.");
                    }

                    var existingByUsername = await _userManager.FindByNameAsync(employeeDto.username);
                    if (existingByUsername != null && existingByUsername.Id != user.Id)
                    {
                        throw new InvalidOperationException("Username is already in use by another user.");
                    }

                    user.Email = employeeDto.Email;
                    user.UserName = employeeDto.username;
                    user.PhoneNumber = employeeDto.PhoneNumber;
                    await _userManager.UpdateAsync(user);
                }

                var selectedRoles = employeeDto.Roles?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();
                if (!selectedRoles.Any())
                {
                    selectedRoles.Add("Employee");
                }

                foreach (var role in selectedRoles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                if (user != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRolesAsync(user, selectedRoles);
                }

                employeeEntity.Name = employeeDto.Name;
                employeeEntity.Salary = employeeDto.Salary;
                employeeEntity.Address = employeeDto.Address;
                employeeEntity.IsActive = employeeDto.IsActive;

                await _employeeRepository.UpdateAsync(employeeEntity);

                var isDriverSelected = selectedRoles.Any(r => string.Equals(r, "Driver", StringComparison.OrdinalIgnoreCase));
                if (isDriverSelected)
                {
                    await CreateOrUpdateDriverAsync(employeeEntity, employeeDto);
                }
                else
                {
                    var existingDriver = await _driverRepository.GetByEmployeeIdAsync(employeeEntity.EmployeeId);
                    if (existingDriver != null)
                    {
                        await _driverRepository.DeleteAsync(existingDriver.DriverId);
                    }
                }

                if (!employeeEntity.IsActive)
                {
                    var driver = await _driverRepository.GetByEmployeeIdAsync(employeeEntity.EmployeeId);
                    if (driver != null && driver.IsActive)
                    {
                        driver.IsActive = false;
                        driver.UpdatedAt = DateTime.UtcNow;
                        await _driverRepository.UpdateAsync(driver);
                    }
                }

                await _auditLogManager.LogAsync("Update", "Employee", employeeDto.EmployeeId.ToString(), $"Updated employee profile: {employeeDto.Name}");
            }
            else
            {
                throw new KeyNotFoundException($"Employee with ID {employeeDto.EmployeeId} not found.");

            }


>>>>>>> Mahmoud-V3
        }


        public async Task DeleteEmployee(int id)
        {
            _logger.LogInformation("Deleting employee {Id}", id);
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with ID {id} not found.");
            }

            var user = await _userManager.FindByIdAsync(employee.aspNetUserId);
            var hasBookings = (await _bookingRepository.GetBookingsByEmployeeIdAsync(id)).Any();
            var hasBlacklistEntries = employee.BlackLists?.Any() ?? false;

            if (hasBookings || hasBlacklistEntries)
            {
                if (employee.IsActive)
                {
                    employee.IsActive = false;
                    await _employeeRepository.UpdateAsync(employee);
                }

<<<<<<< HEAD
=======
                var driver = await _driverRepository.GetByEmployeeIdAsync(employee.EmployeeId);
                if (driver != null && driver.IsActive)
                {
                    driver.IsActive = false;
                    driver.UpdatedAt = DateTime.UtcNow;
                    await _driverRepository.UpdateAsync(driver);
                }

>>>>>>> Mahmoud-V3
                if (user != null)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    await _userManager.UpdateAsync(user);
                }

                throw new InvalidOperationException("Employee has existing activity and was marked as inactive instead of being deleted.");
            }

<<<<<<< HEAD
            await _employeeRepository.DeleteAsync(id);
            
=======
>>>>>>> Mahmoud-V3
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

<<<<<<< HEAD
=======
            var existingDriver = await _driverRepository.GetByEmployeeIdAsync(employee.EmployeeId);
            if (existingDriver != null)
            {
                await _driverRepository.DeleteAsync(existingDriver.DriverId);
            }

            await _employeeRepository.DeleteAsync(id);
>>>>>>> Mahmoud-V3
            await _auditLogManager.LogAsync("Delete", "Employee", id.ToString(), $"Deleted employee {id}");
        }

        public async Task<IList<string>> GetUserRoles(int userId)
        {var employee = await _employeeRepository.GetByIdAsync(userId);
            var user = await _userManager.FindByIdAsync(employee.aspNetUserId);
            if (user != null)
            {
                return await _userManager.GetRolesAsync(user);
            }
            return new List<string>();
        }

        public async Task<bool> SetCustomerActiveStatus(int customerId, bool isActive, int adminEmployeeId)
        {
            var roles = await GetUserRoles(adminEmployeeId);
            if (roles.Contains("Admin"))
            {
                await _customerManager.UpdateActiveStatus(customerId, isActive);
                await _auditLogManager.LogAsync("Update", "Customer", customerId.ToString(), $"Admin changed active status to: {isActive}");
                return true;
            }
            return false;
        }

        public async Task<bool> SetEmployeeActiveStatus(string employeeId, bool isActive, int adminEmployeeId)
        {
            var roles = await GetUserRoles(adminEmployeeId);
            if (roles.Contains("Admin"))
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId);
                if (employee != null)
                {
                    employee.IsActive = isActive;
                    await _employeeRepository.UpdateAsync(employee);
<<<<<<< HEAD
=======

                    var driver = await _driverRepository.GetByEmployeeIdAsync(employee.EmployeeId);
                    if (driver != null)
                    {
                        driver.IsActive = isActive;
                        driver.UpdatedAt = DateTime.UtcNow;
                        await _driverRepository.UpdateAsync(driver);
                    }
>>>>>>> Mahmoud-V3
                    return true;
                }
            }
            return false;
        }

        // Update all employee info
        public async Task UpdateAllEmployeeInfo(string employeeId, string name, decimal? salary, string? address, bool isActive)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee != null)
            {
                employee.Name = name;
                employee.Salary = salary;
                employee.Address = address;
                employee.IsActive = isActive;
                await _employeeRepository.UpdateAsync(employee);
            }
        }

        // Update field by field
        public async Task UpdateEmployeeName(string employeeId, string name)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee != null)
            {
                employee.Name = name;
                await _employeeRepository.UpdateAsync(employee);
            }
        }

        public async Task UpdateEmployeeSalary(string employeeId, decimal? salary)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee != null)
            {
                employee.Salary = salary;
                await _employeeRepository.UpdateAsync(employee);
            }
        }

        public async Task UpdateEmployeeAddress(string employeeId, string? address)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee != null)
            {
                employee.Address = address;
                await _employeeRepository.UpdateAsync(employee);
            }
        }

        public async Task UpdateEmployeeActiveStatus(string employeeId, bool isActive)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee != null)
            {
                employee.IsActive = isActive;
                await _employeeRepository.UpdateAsync(employee);
<<<<<<< HEAD
=======

                var driver = await _driverRepository.GetByEmployeeIdAsync(employee.EmployeeId);
                if (driver != null)
                {
                    driver.IsActive = isActive;
                    driver.UpdatedAt = DateTime.UtcNow;
                    await _driverRepository.UpdateAsync(driver);
                }
>>>>>>> Mahmoud-V3
            }
        }

        public async Task<EmployeeDto?> GetEmployeeByEmail(string email)
        {
            var employees = await _employeeRepository.GetAllAsync();
            var employeeDtos = _mapper.Map<List<EmployeeDto>>(employees);
            return employeeDtos.FirstOrDefault(e => e.Email == email);
        }

        public async Task<EmployeeDto?> GetEmployeeByUsername(string username)
        {
            var employees = await _employeeRepository.GetAllAsync();
            var employeeDtos = _mapper.Map<List<EmployeeDto>>(employees);
            return employeeDtos.FirstOrDefault(e => e.username == username);
        }

        public async Task<List<EmployeeDisplayDto>> GetAllEmployeesWithRoles()
        {
            var employees = await _employeeRepository.GetAllAsync();
            var result = new List<EmployeeDisplayDto>();

            foreach (var emp in employees)
            {
                var user = await _userManager.FindByIdAsync(emp.aspNetUserId);
<<<<<<< HEAD
                var roles = await _userManager.GetRolesAsync(user);
=======
                var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
                var driver = await _driverRepository.GetByEmployeeIdAsync(emp.EmployeeId);
>>>>>>> Mahmoud-V3

                var displayDto = new EmployeeDisplayDto
                {
                    EmployeeId = emp.EmployeeId,
                    Name = emp.Name,
<<<<<<< HEAD
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Salary = emp.Salary,
                    Address = emp.Address,
                    IsActive = emp.IsActive,
                    Role = roles.FirstOrDefault() ?? "N/A"
=======
                    Email = user?.Email ?? string.Empty,
                    PhoneNumber = user?.PhoneNumber ?? string.Empty,
                    Salary = emp.Salary,
                    Address = emp.Address,
                    IsActive = emp.IsActive,
                    Roles = roles.ToList(),
                    IsDriver = roles.Any(r => string.Equals(r, "Driver", StringComparison.OrdinalIgnoreCase)),
                    DriverId = driver?.DriverId,
                    DriverCode = driver?.DriverCode,
                    DriverFullName = driver?.FullName,
                    DriverPhone = driver?.Phone,
                    DriverEmail = driver?.Email,
                    DriverRating = driver?.Rating,
                    DriverLicenseNumber = driver?.LicenseNumber,
                    DriverLicenseExpiry = driver?.LicenseExpiry,
                    DriverLanguages = driver?.Languages,
                    DriverIsActive = driver != null && emp.IsActive && driver.IsActive,

                    DriverCreatedAt = driver?.CreatedAt,
                    DriverUpdatedAt = driver?.UpdatedAt
>>>>>>> Mahmoud-V3
                };

                result.Add(displayDto);
            }

            return result;
        }

<<<<<<< HEAD

        public async Task<List<string>> GetActiveEmployeeEmailsAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();
            var activeEmployees = employees.Where(e => e.IsActive).ToList();
            var emails = new List<string>();

            foreach (var emp in activeEmployees)
            {
                var user = await _userManager.FindByIdAsync(emp.aspNetUserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    emails.Add(user.Email);
                }
            }
            return emails;
        }

        public async Task<List<string>> GetAdminEmailsAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();
            var emails = new List<string>();

            foreach (var emp in employees)
            {
                if (!emp.IsActive) continue;
                
                var user = await _userManager.FindByIdAsync(emp.aspNetUserId);
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin") && !string.IsNullOrEmpty(user.Email))
                {
                    emails.Add(user.Email);
                }
            }
            return emails;
        }

        public async Task<PagedResultDto<EmployeeDisplayDto>> GetEmployeesPagedAsync(
            string? search,
            bool? active,
            string? role,
            int page = 1,
            int pageSize = 10,
            string? sortColumn = "Name",
            string? sortDirection = "asc")
        {
            var query = _employeeRepository.Query()
                .Include(e => e.User)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(e =>
                    e.Name.ToLower().Contains(lowerSearch) ||
                    e.User.Email.ToLower().Contains(lowerSearch) ||
                    e.User.PhoneNumber.ToLower().Contains(lowerSearch));
            }

            if (active.HasValue)
            {
                query = query.Where(e => e.IsActive == active.Value);
            }

            // Apply sorting before pagination
            query = ApplySort(query, sortColumn, sortDirection);

            // If filtering by role, we might need to do it after fetching if we can't join easily
            // However, for performance, if role is specified, we might want to filter first.
            // Given complexity, if role is specified, we'll fetch all matching other criteria, then filter by role memory, then page.
            // If role is NOT specified, we page normally.

            if (!string.IsNullOrEmpty(role))
            {
                // In-memory filter for role (less efficient but simplest without custom joins)
                var allMatches = await query.ToListAsync();
                var roleFiltered = new List<Employee>();
                
                foreach (var emp in allMatches)
                {
                    var user = await _userManager.FindByIdAsync(emp.aspNetUserId);
                    if (user != null && await _userManager.IsInRoleAsync(user, role))
                    {
                        roleFiltered.Add(emp);
                    }
                }

                var totalCountFiltered = roleFiltered.Count;
                var pagedItemsFiltered = roleFiltered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var dtosFiltered = new List<EmployeeDisplayDto>();
                foreach (var emp in pagedItemsFiltered)
                {
                    var user = await _userManager.FindByIdAsync(emp.aspNetUserId);
                    var roles = await _userManager.GetRolesAsync(user);
                     dtosFiltered.Add(new EmployeeDisplayDto
                    {
                        EmployeeId = emp.EmployeeId,
                        Name = emp.Name,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Salary = emp.Salary,
                        Address = emp.Address,
                        IsActive = emp.IsActive,
                        Role = roles.FirstOrDefault() ?? "N/A"
                    });
                }

                return new PagedResultDto<EmployeeDisplayDto>
                {
                    Items = dtosFiltered,
                    TotalCount = totalCountFiltered,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCountFiltered / (double)pageSize)
                };
            }

            // Normal path (no role filter)
            var totalCount = await query.CountAsync();
            var pagedItems = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = new List<EmployeeDisplayDto>();
            foreach (var emp in pagedItems)
            {
                 var user = await _userManager.FindByIdAsync(emp.aspNetUserId);
                 // If user is null (data integrity issue), skip or handle? distinct from query include? 
                 // Include(e => e.User) guarantees it's loaded if EF recognizes relationship.
                 // But _userManager might trigger extra DB calls if we aren't careful.
                 // Since we have the User object from Include, we can try to use it directly if possible, 
                 // but GetRolesAsync needs user.Id or User object.
                 
                 var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();

                 dtos.Add(new EmployeeDisplayDto
                {
                    EmployeeId = emp.EmployeeId,
                    Name = emp.Name,
                    Email = emp.User?.Email ?? "N/A",
                    PhoneNumber = emp.User?.PhoneNumber,
                    Salary = emp.Salary,
                    Address = emp.Address,
                    IsActive = emp.IsActive,
                    Role = roles.FirstOrDefault() ?? "N/A"
                });
            }

            return new PagedResultDto<EmployeeDisplayDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        private IQueryable<Employee> ApplySort(IQueryable<Employee> query, string? sortColumn, string? sortDirection)
        {
            var isAsc = sortDirection?.ToLower() == "asc";
            return sortColumn?.ToLower() switch
            {
                "name" => isAsc ? query.OrderBy(e => e.Name) : query.OrderByDescending(e => e.Name),
                "email" => isAsc ? query.OrderBy(e => e.User.Email) : query.OrderByDescending(e => e.User.Email),
                "salary" => isAsc ? query.OrderBy(e => e.Salary) : query.OrderByDescending(e => e.Salary),
                "isactive" => isAsc ? query.OrderBy(e => e.IsActive) : query.OrderByDescending(e => e.IsActive),
                _ => isAsc ? query.OrderBy(e => e.Name) : query.OrderByDescending(e => e.Name)
            };
        }
=======
        private async Task EnrichEmployeeDtoAsync(EmployeeDto dto, Employee employee)
        {
            var user = await _userManager.FindByIdAsync(employee.aspNetUserId);
            dto.Roles = user != null
                ? (await _userManager.GetRolesAsync(user)).ToList()
                : new List<string>();

            var driver = await _driverRepository.GetByEmployeeIdAsync(employee.EmployeeId);
            if (driver != null)
            {
                dto.DriverId = driver.DriverId;
                dto.DriverCode = driver.DriverCode;
                dto.DriverFullName = driver.FullName;
                dto.DriverPhone = driver.Phone;
                dto.DriverEmail = driver.Email;
                dto.DriverRating = driver.Rating;
                dto.DriverLicenseNumber = driver.LicenseNumber;
                dto.DriverLicenseExpiry = driver.LicenseExpiry;
                dto.DriverLanguages = driver.Languages;
                dto.DriverNotes = driver.Notes;
                dto.DriverIsActive = employee.IsActive && driver.IsActive;
                dto.DriverCreatedAt = driver.CreatedAt;
                dto.DriverUpdatedAt = driver.UpdatedAt;
            }
        }

        private async Task<Driver> CreateOrUpdateDriverAsync(Employee employee, EmployeeCreateDTO dto)
        {
            var driver = await _driverRepository.GetByEmployeeIdAsync(employee.EmployeeId);
            var isNew = driver == null;
            driver ??= new Driver
            {
                CreatedAt = DateTime.UtcNow,
                DriverCode = await GenerateDriverCodeAsync(dto.DriverCode)
            };

            driver.EmployeeId = employee.EmployeeId;
            driver.AspNetUserId = employee.aspNetUserId;
            if (!string.IsNullOrWhiteSpace(dto.DriverCode) && dto.DriverCode != driver.DriverCode)
            {
                driver.DriverCode = await GenerateDriverCodeAsync(dto.DriverCode);
            }
            driver.FullName = string.IsNullOrWhiteSpace(dto.DriverFullName) ? employee.Name : dto.DriverFullName;
            driver.Phone = string.IsNullOrWhiteSpace(dto.DriverPhone) ? dto.PhoneNumber : dto.DriverPhone;
            driver.Email = string.IsNullOrWhiteSpace(dto.DriverEmail) ? dto.Email : dto.DriverEmail;
            driver.Rating = dto.DriverRating;
            driver.LicenseNumber = dto.DriverLicenseNumber;
            driver.LicenseExpiry = dto.DriverLicenseExpiry;
            driver.Languages = dto.DriverLanguages;
            driver.Notes = dto.DriverNotes;
            driver.IsActive = employee.IsActive && dto.DriverIsActive;
            driver.UpdatedAt = DateTime.UtcNow;

            if (isNew)
            {
                await _driverRepository.AddAsync(driver);
            }
            else
            {
                await _driverRepository.UpdateAsync(driver);
            }

            return driver;
        }

        private async Task<Driver> CreateOrUpdateDriverAsync(Employee employee, EmployeeDto dto)
        {
            var driver = await _driverRepository.GetByEmployeeIdAsync(employee.EmployeeId);
            var isNew = driver == null;
            driver ??= new Driver
            {
                CreatedAt = DateTime.UtcNow,
                DriverCode = await GenerateDriverCodeAsync(dto.DriverCode)
            };

            driver.EmployeeId = employee.EmployeeId;
            driver.AspNetUserId = employee.aspNetUserId;
            if (!string.IsNullOrWhiteSpace(dto.DriverCode) && dto.DriverCode != driver.DriverCode)
            {
                driver.DriverCode = await GenerateDriverCodeAsync(dto.DriverCode);
            }
            driver.FullName = string.IsNullOrWhiteSpace(dto.DriverFullName) ? employee.Name : dto.DriverFullName;
            driver.Phone = string.IsNullOrWhiteSpace(dto.DriverPhone) ? dto.PhoneNumber : dto.DriverPhone;
            driver.Email = string.IsNullOrWhiteSpace(dto.DriverEmail) ? dto.Email : dto.DriverEmail;
            driver.Rating = dto.DriverRating;
            driver.LicenseNumber = dto.DriverLicenseNumber;
            driver.LicenseExpiry = dto.DriverLicenseExpiry;
            driver.Languages = dto.DriverLanguages;
            driver.Notes = dto.DriverNotes;
            driver.IsActive = employee.IsActive && dto.DriverIsActive;
            driver.UpdatedAt = DateTime.UtcNow;

            if (isNew)
            {
                await _driverRepository.AddAsync(driver);
            }
            else
            {
                await _driverRepository.UpdateAsync(driver);
            }

            return driver;
        }

        private async Task<string> GenerateDriverCodeAsync(string? requestedCode)
        {
            if (!string.IsNullOrWhiteSpace(requestedCode) && !await _driverRepository.DriverCodeExistsAsync(requestedCode))
            {
                return requestedCode;
            }

            string code;
            do
            {
                code = $"DR-{Random.Shared.Next(1000, 9999)}";
            } while (await _driverRepository.DriverCodeExistsAsync(code));

            return code;
        }

>>>>>>> Mahmoud-V3
    }
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.username, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
<<<<<<< HEAD
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId)) // Explicit map
=======
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.DriverId, opt => opt.Ignore())
                .ForMember(dest => dest.DriverCode, opt => opt.Ignore())
                .ForMember(dest => dest.DriverFullName, opt => opt.Ignore())
                .ForMember(dest => dest.DriverPhone, opt => opt.Ignore())
                .ForMember(dest => dest.DriverEmail, opt => opt.Ignore())
                .ForMember(dest => dest.DriverRating, opt => opt.Ignore())
                .ForMember(dest => dest.DriverLicenseNumber, opt => opt.Ignore())
                .ForMember(dest => dest.DriverLicenseExpiry, opt => opt.Ignore())
                .ForMember(dest => dest.DriverLanguages, opt => opt.Ignore())
                .ForMember(dest => dest.DriverNotes, opt => opt.Ignore())
                .ForMember(dest => dest.DriverIsActive, opt => opt.Ignore())
                .ForMember(dest => dest.DriverCreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DriverUpdatedAt, opt => opt.Ignore())
>>>>>>> Mahmoud-V3
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore()); // Prevent circular reference

            CreateMap<EmployeeCreateDTO, Employee>();
        }
    }

<<<<<<< HEAD
}
=======
}
>>>>>>> Mahmoud-V3
