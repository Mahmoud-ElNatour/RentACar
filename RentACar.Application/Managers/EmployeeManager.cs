using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using Microsoft.Extensions.Logging;

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

        public EmployeeManager(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IEmployeeRepository employeeRepository, IMapper mapper, CustomerManager customerManager, ILogger<EmployeeManager> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _customerManager = customerManager;
            _logger = logger;
        }

        public async Task<EmployeeDto?> CreateEmployee(EmployeeCreateDTO createDto)
        {
            _logger.LogInformation("Creating employee for {Email}", createDto.Email);
            var user = new IdentityUser
            {
                UserName = createDto.Email,
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

                return _mapper.Map<EmployeeDto>(employee);
            }
            _logger.LogWarning("Failed to create employee for {Email}", createDto.Email);
            return null;
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

        public async Task UpdateEmployee(EmployeeDto employeeDto)
        {
            var employeeEntity = await _employeeRepository.GetByIdAsync(employeeDto.EmployeeId);
            if (employeeEntity != null)
            {
                var user = await _userManager.FindByIdAsync(employeeEntity.aspNetUserId);
                if (user != null)
                {
                    user.Email = employeeDto.Email;
                    user.UserName = employeeDto.username;
                    user.PhoneNumber = employeeDto.PhoneNumber;
                    await _userManager.UpdateAsync(user);
                }

                employeeEntity.Name = employeeDto.Name;
                employeeEntity.Salary = employeeDto.Salary;
                employeeEntity.Address = employeeDto.Address;
                employeeEntity.IsActive = employeeDto.IsActive;

                await _employeeRepository.UpdateAsync(employeeEntity);
            }
            else
            {
                throw new KeyNotFoundException($"Employee with ID {employeeDto.EmployeeId} not found.");

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
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            await _employeeRepository.DeleteAsync(id);
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

    }
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.username, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore()); // Prevent circular reference

            CreateMap<EmployeeCreateDTO, Employee>();
        }
    }

}