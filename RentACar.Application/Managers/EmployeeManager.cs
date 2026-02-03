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
        private readonly AuditLogManager _auditLogManager;
        private readonly EmailManager _emailManager;

        public EmployeeManager(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IEmployeeRepository employeeRepository, IBookingRepository bookingRepository, IMapper mapper, CustomerManager customerManager, ILogger<EmployeeManager> logger, AuditLogManager auditLogManager, EmailManager emailManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _employeeRepository = employeeRepository;
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
                if (!await _roleManager.RoleExistsAsync("Employee"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Employee"));
                }

                await _userManager.AddToRoleAsync(user, "Employee");

                var employee = _mapper.Map<Employee>(createDto);
                employee.IsActive = createDto.IsActive;
                employee.aspNetUserId = user.Id;
                await _employeeRepository.AddAsync(employee);
                _logger.LogInformation("Employee created with id {Id}", employee.EmployeeId);

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
        public async Task<bool> ResetPassword(int EmployeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(EmployeeId);
            if (employee == null) return false;
            var user = await _userManager.FindByIdAsync(employee.aspNetUserId);
            if (user == null) return false;
            
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var newPassword = "E@e123456";
            
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
            return _mapper.Map<EmployeeDto>(employee);
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
            
            // Capture Snapshot Before
            var before = new { 
                employeeEntity.Name, 
                employeeEntity.Salary, 
                employeeEntity.Address, 
                employeeEntity.IsActive,
                Email = user?.Email,
                Username = user?.UserName,
                PhoneNumber = user?.PhoneNumber
            };

            employeeEntity.Name = employeeDto.Name;
            employeeEntity.Salary = employeeDto.Salary;
            employeeEntity.Address = employeeDto.Address;
            employeeEntity.IsActive = employeeDto.IsActive;

            await _employeeRepository.UpdateAsync(employeeEntity);
            _logger.LogInformation("Employee {Id} updated successfully in repository.", employeeDto.EmployeeId);

            // Capture Snapshot After
            var after = new { 
                employeeEntity.Name, 
                employeeEntity.Salary, 
                employeeEntity.Address, 
                employeeEntity.IsActive,
                Email = user?.Email,
                Username = user?.UserName,
                PhoneNumber = user?.PhoneNumber
            };

            await _auditLogManager.LogAsync(
                "Update", 
                "Employee", 
                employeeDto.EmployeeId.ToString(), 
                $"Updated employee profile: {employeeDto.Name}",
                oldValues: before,
                newValues: after);
            
            if (oldActive != employeeDto.IsActive)
            {
                string status = employeeEntity.IsActive ? "Activated" : "Deactivated";
                string reason = employeeEntity.IsActive ? "Account activated." : "Account deactivated.";
                await _emailManager.SendAccountStatusEmail(user.Email, employeeEntity.Name, status, reason);
            }
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
                .ForMember(dest => dest.DriverId, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.DriverId : (int?)null))
                .ForMember(dest => dest.DriverCode, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.DriverCode : null))
                .ForMember(dest => dest.DriverFullName, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.FullName : null))
                .ForMember(dest => dest.DriverPhone, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.Phone : null))
                .ForMember(dest => dest.DriverEmail, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.Email : null))
                .ForMember(dest => dest.DriverRating, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.Rating : null))
                .ForMember(dest => dest.DriverLicenseNumber, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.LicenseNumber : null))
                .ForMember(dest => dest.DriverLicenseExpiry, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.LicenseExpiry : null))
                .ForMember(dest => dest.DriverLanguages, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.Languages : null))
                .ForMember(dest => dest.DriverNotes, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.Notes : null))
                .ForMember(dest => dest.DriverIsActive, opt => opt.MapFrom(src => src.Driver != null && src.Driver.IsActive))
                .ForMember(dest => dest.DriverCreatedAt, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.CreatedAt : (DateTime?)null))
                .ForMember(dest => dest.DriverUpdatedAt, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.UpdatedAt : null))
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore()); // Prevent circular reference

            CreateMap<EmployeeCreateDTO, Employee>();
        }
    }

}
