using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using AspNetUserDto = RentACar.Application.DTOs.AspNetUser; // Assuming you have this DTO
using EmployeeDto = RentACar.Application.DTOs.EmployeeDto;        // Assuming you have this DTO
using CustomerDto = RentACar.Application.DTOs.CustomerDTO;        // Assuming you have this DTO
using AspNetUserEntity = RentACar.Core.Entities.AspNetUser;
using EmployeeEntity = RentACar.Core.Entities.Employee;
using CustomerEntity = RentACar.Core.Entities.Customer;

namespace RentACar.Core.Managers
{
    public class BlacklistManager
    {
        private readonly IBlacklistRepository _blacklistRepository;
        private readonly UserManager<AspNetUserEntity> _userManager;
        private readonly IMapper _mapper;
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public BlacklistManager(IBlacklistRepository blacklistRepository, UserManager<AspNetUserEntity> userManager, IMapper mapper, ICustomerRepository customerRepository, IEmployeeRepository employeeRepository)
        {
            _blacklistRepository = blacklistRepository;
            _userManager = userManager;
            _mapper = mapper;
            _customerRepository = customerRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<BlacklistDto?> AddToBlacklistAsync(AddToBlacklistRequestDto requestDto, EmployeeDto loggedInEmployeeDto)
        {
            AspNetUserEntity? userToBlacklist = null;
            if (requestDto.UseUsername)
            {
                userToBlacklist = await _userManager.FindByNameAsync(requestDto.Identifier);
            }
            else
            {
                userToBlacklist = await _userManager.FindByIdAsync(requestDto.Identifier);
            }

            if (userToBlacklist == null)
            {
                return null; // Or throw UserNotFoundException
            }

            // Assuming EmployeeDto has EmployeeId\
            return await AddToBlacklistInternalAsync(userToBlacklist, requestDto.Reason, loggedInEmployeeDto.EmployeeId);
        }
        private async Task<BlacklistDto?> AddToBlacklistInternalAsync(AspNetUserEntity userToBlacklist, string reason, int loggedInEmployeeId)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return null; // Or throw ArgumentException("Reason cannot be empty.");
            }

            var existingBlacklist = await _blacklistRepository.GetByUserIdAsync(userToBlacklist.Id);
            if (existingBlacklist != null)
            {
                return _mapper.Map<BlacklistDto>(existingBlacklist); // Already blacklisted
            }

            var loggedInEmployee = await _employeeRepository.GetByIdAsync(loggedInEmployeeId);
            if (loggedInEmployee?.User == null)
            {
                return null; // Cannot verify who is blacklisting
            }

            var isAdmin = await _userManager.IsInRoleAsync(loggedInEmployee.User, "Admin");
            var isEmployee = await _userManager.IsInRoleAsync(loggedInEmployee.User, "Employee");
            var isCustomer = await _userManager.IsInRoleAsync(loggedInEmployee.User, "Customer");

            if (isCustomer)
            {
                return null; // Customers are not authorized to blacklist
            }

            // Check target user's role
            var isTargetCustomer = await _userManager.IsInRoleAsync(userToBlacklist, "Customer");
            var isTargetEmployee = await _userManager.IsInRoleAsync(userToBlacklist, "Employee");

            // Authorization rules
            if (isEmployee && isTargetEmployee && !isAdmin)
            {
                return null; // Non-admin employees can't blacklist employees
            }

            var blacklistEntry = new BlackList
            {
                UserId = userToBlacklist.Id,
                Reason = reason,
                DateBlocked = DateOnly.FromDateTime(DateTime.UtcNow),
                EmployeeDoneBlacklistId = loggedInEmployeeId
            };

            var addedEntity = await _blacklistRepository.AddAsync(blacklistEntry);

            // Handle user deactivation
            if (isTargetCustomer)
            {
                var customerEntity = await _customerRepository.GetByIdAsync(userToBlacklist.Id);
                if (customerEntity != null)
                {
                    customerEntity.Isactive = false;
                    await _customerRepository.UpdateAsync(customerEntity);
                }
            }
            else if (isTargetEmployee)
            {
                var employeeEntity = await _employeeRepository.GetByIdAsync(userToBlacklist.Id);
                if (employeeEntity != null)
                {
                    employeeEntity.IsActive = false;
                    await _employeeRepository.UpdateAsync(employeeEntity);
                }
            }

            return _mapper.Map<BlacklistDto>(addedEntity);
        }

        public async Task<bool> RemoveFromBlacklistAsync(RemoveFromBlacklistRequestDto requestDto, EmployeeDto loggedInEmployeeDto)
        {
            AspNetUserEntity? userToRemove = null;
            if (requestDto.UseUsername)
            {
                userToRemove = await _userManager.FindByNameAsync(requestDto.Identifier);
            }
            else
            {
                userToRemove = await _userManager.FindByIdAsync(requestDto.Identifier);
            }

            if (userToRemove == null)
            {
                return false; // Or throw UserNotFoundException
            }

            var blacklistEntry = await _blacklistRepository.GetByUserIdAsync(userToRemove.Id);
            if (blacklistEntry == null)
            {
                return false; // User is not blacklisted
            }

            await _blacklistRepository.DeleteAsync(blacklistEntry);

            if (await _userManager.IsInRoleAsync(userToRemove, "Customer"))
            {
                var customerEntity = await _customerRepository.GetByIdAsync(userToRemove.Id);
                if (customerEntity != null)
                {
                    customerEntity.Isactive = true;
                    await _customerRepository.UpdateAsync(customerEntity);
                }
            }
            else if (await _userManager.IsInRoleAsync(userToRemove, "Employee"))
            {
                // Fetch the EmployeeEntity of the logged-in employee to access their User (AspNetUser)
                var loggedInEmployeeEntity = await _employeeRepository.GetByIdAsync(loggedInEmployeeDto.EmployeeId);
                if (loggedInEmployeeEntity?.User != null)
                {
                    var isAdmin = await _userManager.IsInRoleAsync(loggedInEmployeeEntity.User, "Admin");
                    if (isAdmin)
                    {
                        var employeeEntity = await _employeeRepository.GetByIdAsync(userToRemove.Id);
                        if (employeeEntity != null)
                        {
                            employeeEntity.IsActive = true;
                            await _employeeRepository.UpdateAsync(employeeEntity);
                        }
                    }
                    else
                    {
                        // Normal employee cannot remove another employee from blacklist
                        return false; // Or throw UnauthorizedAccessException
                    }
                }
                else
                {
                    // Could not retrieve logged-in employee's User information
                    return false; // Or throw an appropriate exception
                }
            }

            return true;
        }

        public async Task<BlacklistDto?> GetBlacklistByUserIdAsync(String userId)
        {
            var blacklistEntry = await _blacklistRepository.GetByUserIdAsync(userId);
            return _mapper.Map<BlacklistDto>(blacklistEntry);
        }
    }

    public class BlacklistProfile : Profile
    {
        public BlacklistProfile()
        {
            CreateMap<BlackList, BlacklistDto>().ReverseMap();
            //CreateMap<AddToBlacklistRequestDto, BlackList>()
            //    .ForMember(dest => dest.DateBlocked, opt => opt.MapFrom(src => DateOnly.FromDateTime(DateTime.UtcNow)))
            //    .ForMember(dest => dest.EmployeeDoneBlacklistId, opt => opt.Ignore()); // Set this in the manager
            //CreateMap<RemoveFromBlacklistRequestDto, BlackList>()
            //    .ForMember(dest => dest.DateBlocked, opt => opt.Ignore())
            //    .ForMember(dest => dest.EmployeeDoneBlacklistId, opt => opt.Ignore());

        }
    }

}