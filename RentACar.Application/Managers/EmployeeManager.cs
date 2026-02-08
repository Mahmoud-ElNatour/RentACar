using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Application.Managers
{
    public class EmployeeManager
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly CustomerManager _customerManager; // To access CustomerManager methods
        private readonly ILogger<EmployeeManager> _logger;
        private readonly IBookingRepository _bookingRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IDriverAllowedCategoryRepository _driverAllowedCategoryRepository;
        private readonly AuditLogManager _auditLogManager;
        private readonly EmailManager _emailManager;

        public EmployeeManager(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IEmployeeRepository employeeRepository, IDriverRepository driverRepository, IDriverAllowedCategoryRepository driverAllowedCategoryRepository, IBookingRepository bookingRepository, IMapper mapper, CustomerManager customerManager, ILogger<EmployeeManager> logger, AuditLogManager auditLogManager, EmailManager emailManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _employeeRepository = employeeRepository;
            _driverRepository = driverRepository;
            _driverAllowedCategoryRepository = driverAllowedCategoryRepository;
            _mapper = mapper;
            _customerManager = customerManager;
            _logger = logger;
            _bookingRepository = bookingRepository;
            _auditLogManager = auditLogManager;
            _emailManager = emailManager;
        }

        public async Task<EmployeeDto?> CreateEmployee(EmployeeCreateDTO createDto)
        {
            _logger.LogInformation("Creating employee for {Email}", createDto.Email);

            // 1. Validate role combinations
            var requestedRoles = createDto.Roles ?? new List<string> { "Employee" };
            ValidateRoles(requestedRoles);
            var requiresDriverCategories = requestedRoles.Contains("Driver", StringComparer.OrdinalIgnoreCase);
            if (requiresDriverCategories && (createDto.AllowedCategoryIds == null || !createDto.AllowedCategoryIds.Any()))
            {
                throw new InvalidOperationException("Driver must be assigned to at least one category.");
            }

            var username = createDto.Email;

            var existingByUsername = await _userManager.FindByNameAsync(username);
            if (existingByUsername != null) throw new InvalidOperationException("Username is already in use.");

            var existingByEmail = await _userManager.FindByEmailAsync(createDto.Email);
            if (existingByEmail != null) throw new InvalidOperationException("Email address is already registered.");

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
                Employee? employee = null;
                try
                {
                    // 2. Assign Identity Roles
                    foreach (var role in requestedRoles)
                    {
                        if (!await _roleManager.RoleExistsAsync(role))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(role));
                        }
                        await _userManager.AddToRoleAsync(user, role);
                    }

                    employee = _mapper.Map<Employee>(createDto);
                    employee.IsActive = createDto.IsActive;
                    employee.aspNetUserId = user.Id;
                    await _employeeRepository.AddAsync(employee);

                    // 3. Handle Driver Sync
                    if (requestedRoles.Contains("Driver", StringComparer.OrdinalIgnoreCase))
                    {
                        var driver = await SyncDriverRecord(employee, createDto, user.Id);
                        if (driver != null)
                        {
                            await _driverAllowedCategoryRepository.SetAllowedCategoriesAsync(driver.DriverId, createDto.AllowedCategoryIds);
                        }
                    }

                    _logger.LogInformation("Employee created with id {Id}", employee.EmployeeId);
                    await _auditLogManager.LogAsync("Create", "Employee", employee.EmployeeId.ToString(), $"Created new employee: {employee.Name} with roles [{string.Join(", ", requestedRoles)}]");

                    return await GetEmployeeById(employee.EmployeeId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to complete employee creation for {Email}. Rolling back.", createDto.Email);
                    
                    // Attempt to rollback to avoid orphaned account
                    try 
                    { 
                        if (employee != null && employee.EmployeeId > 0)
                        {
                             await _employeeRepository.DeleteAsync(employee.EmployeeId);
                        }
                        await _userManager.DeleteAsync(user); 
                    }
                    catch (Exception rollbackEx) 
                    { 
                        _logger.LogError(rollbackEx, "Rollback failed for {Email}", createDto.Email);
                    }
                    throw;
                }
            }

            var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errorMessage ?? "Unable to create employee account.");
        }
        public async Task<bool> ResetPassword(int EmployeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(EmployeeId);
            if (employee == null) return false;
            var user = await _userManager.FindByIdAsync(employee.aspNetUserId);
            if (user == null) return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var newPassword = $"RentCar{new Random().Next(100000, 999999)}!";

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                await _emailManager.SendAdminResetPasswordEmail(user.Email, newPassword, employee.Name);
                await _auditLogManager.LogEventAsync("Employee.PasswordReset", "Employee", EmployeeId.ToString(), "Admin reset employee password", null, "Success");
            }

            return result.Succeeded;
        }
        public async Task<EmployeeDto?> GetEmployeeById(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            var dto = _mapper.Map<EmployeeDto>(employee);
            if (employee?.Driver != null)
            {
                dto.AllowedCategoryIds = await _driverAllowedCategoryRepository.GetAllowedCategoryIdsByDriverIdAsync(employee.Driver.DriverId);
            }
            return dto;
        }

        public async Task<List<EmployeeDto>> GetAllEmployees()
        {
            var employees = await _employeeRepository.GetAllAsync();
            return _mapper.Map<List<EmployeeDto>>(employees);
        }

        public async Task<EmployeeDto?> GetEmployeeByAspNetUserId(string aspNetUserId)
        {
            var employee = await _employeeRepository.GetByIdAsync(aspNetUserId);
            return _mapper.Map<EmployeeDto>(employee);
        }

        public async Task UpdateEmployee(EmployeeDto employeeDto)
        {
            _logger.LogInformation("Updating employee {Id}", employeeDto.EmployeeId);

            var employeeEntity = await _employeeRepository.Query()
                .Include(e => e.User)
                .Include(e => e.Driver)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeDto.EmployeeId);

            if (employeeEntity == null) throw new KeyNotFoundException($"Employee with ID {employeeDto.EmployeeId} not found.");

            // 1. Sync Identity User & Roles
            var user = await _userManager.FindByIdAsync(employeeEntity.aspNetUserId);
            if (user != null)
            {
                user.Email = employeeDto.Email;
                user.UserName = employeeDto.username ?? employeeDto.Email;
                user.PhoneNumber = employeeDto.PhoneNumber;
                await _userManager.UpdateAsync(user);

                if (employeeDto.Roles != null && employeeDto.Roles.Any())
                {
                    ValidateRoles(employeeDto.Roles);
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var rolesToAdd = employeeDto.Roles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToList();
                    var rolesToRemove = currentRoles.Except(employeeDto.Roles, StringComparer.OrdinalIgnoreCase).ToList();

                    foreach (var r in rolesToAdd)
                    {
                        if (!await _roleManager.RoleExistsAsync(r)) await _roleManager.CreateAsync(new IdentityRole(r));
                        await _userManager.AddToRoleAsync(user, r);
                    }
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }
            }

            // 2. Update Employee Basic Info
            var before = new { employeeEntity.Name, employeeEntity.Salary, employeeEntity.Address, employeeEntity.IsActive };

            employeeEntity.Name = employeeDto.Name;
            employeeEntity.Salary = employeeDto.Salary;
            employeeEntity.Address = employeeDto.Address;
            var oldActive = employeeEntity.IsActive;
            employeeEntity.IsActive = employeeDto.IsActive;

            await _employeeRepository.UpdateAsync(employeeEntity);

            // 3. Sync Driver Data
            bool isDriver = employeeDto.Roles?.Contains("Driver", StringComparer.OrdinalIgnoreCase) ?? false;
            if (isDriver && (employeeDto.AllowedCategoryIds == null || !employeeDto.AllowedCategoryIds.Any()))
            {
                throw new InvalidOperationException("Driver must be assigned to at least one category.");
            }

            var driver = await SyncDriverRecord(employeeEntity, employeeDto, employeeEntity.aspNetUserId, isDriver);
            if (isDriver && driver != null)
            {
                await _driverAllowedCategoryRepository.SetAllowedCategoriesAsync(driver.DriverId, employeeDto.AllowedCategoryIds);
            }

            await _auditLogManager.LogAsync("Update", "Employee", employeeDto.EmployeeId.ToString(), $"Updated employee profile: {employeeDto.Name}", oldValues: before, newValues: employeeDto);

            if (oldActive != employeeDto.IsActive && user != null)
            {
                string status = employeeEntity.IsActive ? "Activated" : "Deactivated";
                string reason = employeeEntity.IsActive ? "Account activated." : "Account deactivated.";
                await _emailManager.SendAccountStatusEmail(user.Email, employeeEntity.Name, status, reason);
            }
        }

        private void ValidateRoles(List<string> roles)
        {
            bool isCustomer = roles.Contains("Customer", StringComparer.OrdinalIgnoreCase);
            bool isEmployee = roles.Contains("Employee", StringComparer.OrdinalIgnoreCase);
            bool isDriver = roles.Contains("Driver", StringComparer.OrdinalIgnoreCase);
            bool isAdmin = roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);

            if (isCustomer && (isEmployee || isDriver))
                throw new InvalidOperationException("Customer role cannot be combined with Employee or Driver roles.");

            if (isAdmin && isCustomer)
                throw new InvalidOperationException("Admin role cannot be combined with Customer role.");
        }

        private async Task<Driver?> SyncDriverRecord(Employee employee, object dto, string aspNetUserId, bool shouldBeActive = true)
        {
            var driver = await _driverRepository.GetByEmployeeIdAsync(employee.EmployeeId);

            // Map common fields from object (DTO or CreateDTO)
            string? fullName = GetPropValue(dto, "DriverFullName")?.ToString() ?? employee.Name;
            decimal? dailyFee = GetPropValue(dto, "DriverDailyFeePerDay") as decimal?;
            string? license = GetPropValue(dto, "DriverLicenseNumber")?.ToString();
            DateOnly? expiry = GetPropValue(dto, "DriverLicenseExpiry") as DateOnly?;
            string? languages = GetPropValue(dto, "DriverLanguages")?.ToString();
            string? notes = GetPropValue(dto, "DriverNotes")?.ToString();

            if (driver == null && shouldBeActive)
            {
                driver = new Driver
                {
                    EmployeeId = employee.EmployeeId,
                    AspNetUserId = aspNetUserId,
                    FullName = fullName,
                    DailyFeePerDay = dailyFee ?? 20m,
                    LicenseNumber = license,
                    LicenseExpiry = expiry,
                    Languages = languages,
                    Notes = notes,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _driverRepository.AddAsync(driver);
            }
            else if (driver != null)
            {
                if (shouldBeActive)
                {
                    driver.FullName = fullName;
                    driver.DailyFeePerDay = dailyFee ?? driver.DailyFeePerDay;
                    driver.LicenseNumber = license ?? driver.LicenseNumber;
                    driver.LicenseExpiry = expiry ?? driver.LicenseExpiry;
                    driver.Languages = languages ?? driver.Languages;
                    driver.Notes = notes ?? driver.Notes;
                    driver.IsActive = true;
                    driver.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    driver.IsActive = false;
                    driver.UpdatedAt = DateTime.UtcNow;
                }
                await _driverRepository.UpdateAsync(driver);
            }

            return driver;
        }

        private object? GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName)?.GetValue(src, null);
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

                if (user != null)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    await _userManager.UpdateAsync(user);
                }

                throw new InvalidOperationException("Employee has existing activity and was marked as inactive instead of being deleted.");
            }

            await _employeeRepository.DeleteAsync(id);

            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            await _auditLogManager.LogAsync("Delete", "Employee", id.ToString(), $"Deleted employee {id}");
        }

        public async Task<IList<string>> GetUserRoles(int userId)
        {
            var employee = await _employeeRepository.GetByIdAsync(userId);
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
                var roles = await _userManager.GetRolesAsync(user);

                var displayDto = new EmployeeDisplayDto
                {
                    EmployeeId = emp.EmployeeId,
                    Name = emp.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Salary = emp.Salary,
                    Address = emp.Address,
                    IsActive = emp.IsActive,
                    Role = roles.FirstOrDefault() ?? "N/A"
                };

                result.Add(displayDto);
            }

            return result;
        }


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
    }
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.username, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId)) // Explicit map
                .ForMember(dest => dest.DriverId, opt => opt.MapFrom(src => src.Driver != null ? (int?)src.Driver.DriverId : null))
                .ForMember(dest => dest.DriverFullName, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.FullName : null))
                .ForMember(dest => dest.DriverPhone, opt => opt.MapFrom(src => src.User != null ? src.User.PhoneNumber : null))
                .ForMember(dest => dest.DriverEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.DriverDailyFeePerDay, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.DailyFeePerDay : (decimal?)null))
                .ForMember(dest => dest.DriverLicenseNumber, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.LicenseNumber : null))
                .ForMember(dest => dest.DriverLicenseExpiry, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.LicenseExpiry : null))
                .ForMember(dest => dest.DriverLanguages, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.Languages : null))
                .ForMember(dest => dest.DriverNotes, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.Notes : null))
                .ForMember(dest => dest.DriverIsActive, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.IsActive : false))
                .ForMember(dest => dest.DriverCreatedAt, opt => opt.MapFrom(src => src.Driver != null ? (DateTime?)src.Driver.CreatedAt : null))
                .ForMember(dest => dest.DriverUpdatedAt, opt => opt.MapFrom(src => src.Driver != null ? (DateTime?)src.Driver.UpdatedAt : null))
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Driver, opt => opt.Ignore());

            CreateMap<EmployeeCreateDTO, Employee>();
        }
    }

}
