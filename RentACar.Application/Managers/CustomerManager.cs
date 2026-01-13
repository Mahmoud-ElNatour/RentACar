using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using Microsoft.Extensions.Logging;
using AspNetUser = RentACar.Application.DTOs.AspNetUser;

namespace RentACar.Application.Managers
{
    public class CustomerManager
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerManager> _logger;
        private readonly IBookingRepository _bookingRepository;
        private readonly AuditLogManager _auditLogManager;

        public CustomerManager(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ICustomerRepository customerRepository,
            IBookingRepository bookingRepository,
            IMapper mapper,
            ILogger<CustomerManager> logger,
            AuditLogManager auditLogManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _customerRepository = customerRepository;
            _mapper = mapper;
            _logger = logger;
            _bookingRepository = bookingRepository;
            _auditLogManager = auditLogManager;
        }
        public async Task<CustomerDTO?> CreateCustomer(CustomerCreateDTO createDto)
        {
            _logger.LogInformation("Creating customer for {Email}", createDto.Email);
            if (string.IsNullOrWhiteSpace(createDto.Username))
            {
                createDto.Username = createDto.Email;
            }

            var existingByUsername = await _userManager.FindByNameAsync(createDto.Username);
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
                UserName = createDto.Username,
                Email = createDto.Email,
                PhoneNumber = createDto.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, createDto.Password);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = "Unable to create user account for the customer.";
                }
                _logger.LogWarning("Failed to create user for {Email}: {Error}", createDto.Email, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Ensure "Customer" role exists
            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Customer"));
            }

            // ✅ Re-fetch user and set email confirmed
            var createdUser = await _userManager.FindByEmailAsync(createDto.Email);
            if (createdUser != null)
            {
                createdUser.EmailConfirmed = true;
                await _userManager.UpdateAsync(createdUser);
            }


            // Assign the user to "Customer" role

            await _userManager.AddToRoleAsync(user, "Customer");


            var customer = new Customer
            {
                aspNetUserId = user.Id,
                Name = createDto.Name,
                Address = createDto.Address,
                DrivingLicenseFront = createDto.DrivingLicenseFront,
                DrivingLicenseBack = createDto.DrivingLicenseBack,
                NationalIdfront = createDto.NationalIdfront,
                NationalIdback = createDto.NationalIdback,
                IsVerified = false,
                Isactive = true
            };

            await _customerRepository.AddAsync(customer);

            _logger.LogInformation("Customer created with id {Id}", customer.UserId);

            await _auditLogManager.LogAsync("Create", "Customer", customer.UserId.ToString(), $"Registered new customer: {customer.Name} ({createDto.Email})");
            
            return _mapper.Map<CustomerDTO>(customer);
        }

        public async Task<IdentityUser?> GetIdentityUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }


        public async Task<CustomerDTO?> GetCustomerById(int id)
        {
            _logger.LogInformation("Fetching customer {Id}", id);
            var customer = await _customerRepository.GetByIdAsync(id);
            return _mapper.Map<CustomerDTO>(customer);
        }

        public async Task<CustomerDTO?> GetCustomerByAspNetUserId(string aspNetUserId)
        {
            _logger.LogInformation("Fetching customer by aspNetUserId {UserId}", aspNetUserId);
            var customer = await _customerRepository.GetByIdAsync(aspNetUserId);
            return _mapper.Map<CustomerDTO>(customer);
        }

        public async Task<List<CustomerDTO>> GetAllCustomers()
        {
            var customers = await _customerRepository.GetAllAsync();
            return _mapper.Map<List<CustomerDTO>>(customers);
        }

        public async Task<List<CustomerDTO>> SearchCustomersByName(string name)
        {
            var customers = await _customerRepository.FindByNameAsync(name);
            return _mapper.Map<List<CustomerDTO>>(customers);
        }

        // update all customer info (used if we do page that have all field and we should filled them all so update all directly)
        public async Task UpdateAllCustomerInfo(string customerId, string name, string address, byte[] drivingLicenseFront, byte[] drivingLicenseBack, byte[] nationalIdFront, byte[] nationalIdBack)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer != null)
            {
                customer.Name = name;
                customer.Address = address;
                customer.DrivingLicenseFront = drivingLicenseFront;
                customer.DrivingLicenseBack = drivingLicenseBack;
                customer.NationalIdfront = nationalIdFront;
                customer.NationalIdback = nationalIdBack;
                await _customerRepository.UpdateAsync(customer);
            }
        }

        // update field by field

        public async Task UpdateVerificationStatus(int customerId, bool isVerified)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer != null)
            {
                customer.IsVerified = isVerified;
                await _customerRepository.UpdateAsync(customer);
                await _auditLogManager.LogAsync("Update", "Customer", customerId.ToString(), $"Updated verification status to: {isVerified}");
            }
        }

        public async Task UpdateCustomerName(int customerId, String name)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer != null)
            {
                customer.Name = name;
                await _customerRepository.UpdateAsync(customer);
            }

        }

        public async Task UpdateActiveStatus(int customerId, bool isActive)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer != null)
            {
                customer.Isactive = isActive;
                await _customerRepository.UpdateAsync(customer);
            }
        }
        public async Task UpdateCustomerAddress(int customerId, string newAddress)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer != null)
            {
                customer.Address = newAddress;
                await _customerRepository.UpdateAsync(customer);
            }
        }
        public async Task UpdateCustomerDrivingLicense(int customerId, byte[] drivingLicenseFront, byte[] drivingLicenseBack)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer != null)
            {
                customer.DrivingLicenseFront = drivingLicenseFront;
                customer.DrivingLicenseBack = drivingLicenseBack;
                await _customerRepository.UpdateAsync(customer);
            }
        }
        public async Task UpdateCustomerNationalId(int customerId, byte[] nationalIdFront, byte[] nationalIdBack)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer != null)
            {
                customer.NationalIdfront = nationalIdFront;
                customer.NationalIdback = nationalIdBack;
                await _customerRepository.UpdateAsync(customer);
            }
        }
      


        public async Task<CustomerDTO?> GetCustomerByEmail(string email)
        {
            var customer = await _customerRepository.GetAllAsync();
            var customerDto = _mapper.Map<List<CustomerDTO>>(customer);
            return customerDto.Find(c => c.Email == email);
        }
        public async Task<CustomerDTO?> GetCustomerByUsername(string username)
        {
            var customer = await _customerRepository.GetAllAsync();
            var customerDto = _mapper.Map<List<CustomerDTO>>(customer);
            return customerDto.Find(c => c.username == username);
        }


        public async Task DeleteCustomer(int id)
        {
            _logger.LogInformation("Deleting customer {Id}", id);
            var customerEntity = await _customerRepository.GetByIdAsync(id);
            if (customerEntity == null)
                throw new Exception("Customer not found");

            var user = await _userManager.FindByIdAsync(customerEntity.aspNetUserId);
            if (user == null)
                throw new Exception("User not found");

            var hasBookings = (await _bookingRepository.GetBookingsByCustomerIdAsync(id)).Any();
            if (hasBookings)
            {
                if (customerEntity.Isactive)
                {
                    customerEntity.Isactive = false;
                    await _customerRepository.UpdateAsync(customerEntity);
                }

                if (user != null)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    await _userManager.UpdateAsync(user);
                }

                throw new InvalidOperationException("Customer has existing activity and was marked as inactive instead of being deleted.");
            }

            await _customerRepository.DeleteAsync(id);
            await _userManager.DeleteAsync(user);
            await _auditLogManager.LogAsync("Delete", "Customer", id.ToString(), $"Deleted customer account: {customerEntity.Name}");
        }


        public async Task UpdateCustomer(CustomerDTO dto)
        {
            _logger.LogInformation("Updating customer {Id}", dto.UserId);
            var customer = await _customerRepository.GetByIdAsync(dto.UserId);
            if (customer != null)
            {
                var user = await _userManager.FindByIdAsync(customer.aspNetUserId);
                if (user != null)
                {
                    dto.username ??= dto.Email;
                    var existingByEmail = await _userManager.FindByEmailAsync(dto.Email);
                    if (existingByEmail != null && existingByEmail.Id != user.Id)
                    {
                        throw new InvalidOperationException("Email address is already registered to another user.");
                    }

                    var existingByUsername = await _userManager.FindByNameAsync(dto.username);
                    if (existingByUsername != null && existingByUsername.Id != user.Id)
                    {
                        throw new InvalidOperationException("Username is already in use by another user.");
                    }

                    user.Email = dto.Email;
                    user.UserName = dto.username;
                    user.PhoneNumber = dto.PhoneNumber;
                    await _userManager.UpdateAsync(user);
                }

                customer.Name = dto.Name;
                customer.Address = dto.Address;
                customer.IsVerified = dto.IsVerified;
                customer.Isactive = dto.Isactive;

                await _customerRepository.UpdateAsync(customer);
                await _auditLogManager.LogAsync("Update", "Customer", dto.UserId.ToString(), $"Updated profile details for: {customer.Name}");
            }
        }

        public async Task<bool> ResetPassword(int customerId, string newPassword)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null) return false;
            var user = await _userManager.FindByIdAsync(customer.aspNetUserId);
            if (user == null) return false;
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }

        public async Task UpdateCustomerDocuments(int customerId, CustomerDocumentsDto docs)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
            }

            var hasChanges = false;

            if (docs.DrivingLicenseFront != null)
            {
                customer.DrivingLicenseFront = docs.DrivingLicenseFront;
                hasChanges = true;
            }

            if (docs.DrivingLicenseBack != null)
            {
                customer.DrivingLicenseBack = docs.DrivingLicenseBack;
                hasChanges = true;
            }

            if (docs.NationalIdfront != null)
            {
                customer.NationalIdfront = docs.NationalIdfront;
                hasChanges = true;
            }

            if (docs.NationalIdback != null)
            {
                customer.NationalIdback = docs.NationalIdback;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _customerRepository.UpdateAsync(customer);
            }
        }
        public async Task<List<CustomerListDto>> GetAllCustomersForListAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            return _mapper.Map<List<CustomerListDto>>(customers);
        }
    }

    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerDTO>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.username, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore()); // skip reverse mapping User
                
            CreateMap<Customer, CustomerListDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.username, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber));
        }
    }

}
