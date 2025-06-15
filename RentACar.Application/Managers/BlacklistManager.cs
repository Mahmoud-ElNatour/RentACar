using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using AspNetUserDto = RentACar.Application.DTOs.AspNetUser; // Assuming you have this DTO
using EmployeeDto = RentACar.Application.DTOs.EmployeeDto;        // Assuming you have this DTO
using CustomerDto = RentACar.Application.DTOs.CustomerDTO;        // Assuming you have this DTO
//using AspNetUserEntity = RentACar.Core.Entities.AspNetUser;
using EmployeeEntity = RentACar.Core.Entities.Employee;
using CustomerEntity = RentACar.Core.Entities.Customer;

namespace RentACar.Application.Managers
{
    public class BlacklistManager
    {
        private readonly IBlacklistRepository _blacklistRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public BlacklistManager(IBlacklistRepository blacklistRepository, UserManager<IdentityUser> userManager, IMapper mapper, ICustomerRepository customerRepository, IEmployeeRepository employeeRepository)
        {
            _blacklistRepository = blacklistRepository;
            _userManager = userManager;
            _mapper = mapper;
            _customerRepository = customerRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<BlacklistDto?> AddToBlacklistAsync(AddToBlacklistRequestDto requestDto, EmployeeDto loggedInEmployeeDto)
        {
            IdentityUser? userToBlacklist = null;
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
        private async Task<BlacklistDto?> AddToBlacklistInternalAsync(IdentityUser userToBlacklist, string reason, int loggedInEmployeeId)
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
            var user = _userManager.FindByIdAsync(loggedInEmployee.aspNetUserId).Result;
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");

            if (isCustomer)
            {
                return null; // Customers are not authorized to blacklist
            }

            // Check target user's role
            var isTargetCustomer = await _userManager.IsInRoleAsync(userToBlacklist, "Customer");
            var isTargetEmployee = await _userManager.IsInRoleAsync(userToBlacklist, "Employee");
            var isTargetAdmin = await _userManager.IsInRoleAsync(userToBlacklist, "Admin");

            // Authorization rules
            if (!isAdmin && isEmployee && (isTargetEmployee || isTargetAdmin))
            {
                // Employees without admin role can only blacklist customers
                return null;
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
            IdentityUser? userToRemove = null;
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

            var targetIsCustomer = await _userManager.IsInRoleAsync(userToRemove, "Customer");
            var targetIsEmployee = await _userManager.IsInRoleAsync(userToRemove, "Employee");
            var targetIsAdmin = await _userManager.IsInRoleAsync(userToRemove, "Admin");

            if (targetIsCustomer)
            {
                var customerEntity = await _customerRepository.GetByIdAsync(userToRemove.Id);
                if (customerEntity != null)
                {
                    customerEntity.Isactive = true;
                    await _customerRepository.UpdateAsync(customerEntity);
                }
            }
            else if (targetIsEmployee || targetIsAdmin)
            {
                // Fetch the EmployeeEntity of the logged-in employee to access their User (AspNetUser)
                var loggedInEmployeeEntity = await _employeeRepository.GetByIdAsync(loggedInEmployeeDto.EmployeeId);
                if (loggedInEmployeeEntity?.User != null)
                {
                    var user = _userManager.FindByIdAsync(loggedInEmployeeEntity.aspNetUserId).Result;
                    var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                    if (isAdmin)
                    {
                        if (targetIsEmployee)
                        {
                            var employeeEntity = await _employeeRepository.GetByIdAsync(userToRemove.Id);
                            if (employeeEntity != null)
                            {
                                employeeEntity.IsActive = true;
                                await _employeeRepository.UpdateAsync(employeeEntity);
                            }
                        }
                        // If target is admin only, nothing to update besides blacklist entry
                    }
                    else
                    {
                        // Normal employee cannot remove admins or employees from blacklist
                        return false; // Unauthorized
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

        public async Task<BlacklistDto?> GetByIdAsync(int id)
        {
            var entry = await _blacklistRepository.GetByIdAsync(id);
            return entry == null ? null : _mapper.Map<BlacklistDto>(entry);
        }

        public async Task<List<BlacklistDisplayDto>> GetAllAsync(string? type = null, string? search = null, int offset = 0, int limit = 30)
        {
            var all = await _blacklistRepository.GetAllAsync();
            var result = new List<BlacklistDisplayDto>();

            foreach (var item in all)
            {
                var user = await _userManager.FindByIdAsync(item.UserId);
                if (user == null) continue;
                var emp = await _employeeRepository.GetByIdAsync(item.EmployeeDoneBlacklistId);

                var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
                var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");
                var userType = isCustomer ? "Customer" : isEmployee ? "Employee" : "Unknown";

                if (!string.IsNullOrEmpty(type) && !string.Equals(type, userType, StringComparison.OrdinalIgnoreCase))
                    continue;

                var display = new BlacklistDisplayDto
                {
                    BlacklistId = item.BlacklistId,
                    UserId = item.UserId,
                    Username = user.UserName ?? string.Empty,
                    Reason = item.Reason,
                    DateBlocked = item.DateBlocked,
                    EmployeeDoneBlacklistId = item.EmployeeDoneBlacklistId,
                    EmployeeName = emp?.Name ?? string.Empty,
                    UserType = userType
                };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLowerInvariant();
                    var match = (display.Username?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                                (display.Reason?.ToLowerInvariant().Contains(searchLower) ?? false);
                    if (!match) continue;
                }

                result.Add(display);
            }

            return result.Skip(offset).Take(limit).ToList();
        }

        public async Task UpdateBlacklistAsync(BlacklistDto dto)
        {
            var entry = await _blacklistRepository.GetByIdAsync(dto.BlacklistId);
            if (entry != null)
            {
                entry.Reason = dto.Reason;
                entry.DateBlocked = dto.DateBlocked;
                entry.EmployeeDoneBlacklistId = dto.EmployeeDoneBlacklistId;
                await _blacklistRepository.UpdateAsync(entry);
            }
        }

        public async Task<bool> RemoveByIdAsync(int id, EmployeeDto loggedInEmployeeDto)
        {
            var entry = await _blacklistRepository.GetByIdAsync(id);
            if (entry == null)
                return false;
            var req = new RemoveFromBlacklistRequestDto { Identifier = entry.UserId, UseUsername = false };
            return await RemoveFromBlacklistAsync(req, loggedInEmployeeDto);
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