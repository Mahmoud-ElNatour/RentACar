using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using Microsoft.EntityFrameworkCore;
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
        private readonly EmailManager _emailManager;

        public CustomerManager(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ICustomerRepository customerRepository,
            IBookingRepository bookingRepository,
            IMapper mapper,
            ILogger<CustomerManager> logger,
            AuditLogManager auditLogManager,
            EmailManager emailManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _customerRepository = customerRepository;
            _mapper = mapper;
            _logger = logger;
            _bookingRepository = bookingRepository;
            _auditLogManager = auditLogManager;
            _emailManager = emailManager;
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

            await _auditLogManager.LogEventAsync("Customer.Registered", "Customer", customer.UserId.ToString(), $"Registered new customer: {customer.Name} ({createDto.Email})", null, "Success");
            
            return _mapper.Map<CustomerDTO>(customer);
        }

        public async Task<CustomerDTO?> CreateCustomerForExternalUser(IdentityUser user, CustomerCreateDTO createDto)
        {
            _logger.LogInformation("Creating customer profile for external user {Email}", user.Email);

            // Ensure "Customer" role exists and assign
            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Customer"));
            }
            if (!await _userManager.IsInRoleAsync(user, "Customer"))
            {
                await _userManager.AddToRoleAsync(user, "Customer");
            }

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

            _logger.LogInformation("Customer profile created for external user {Id}", customer.UserId);

            await _auditLogManager.LogEventAsync("Customer.RegisteredExternal", "Customer", customer.UserId.ToString(), $"Registered new external customer: {customer.Name} ({user.Email})", null, "Success");

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
             await UpdateVerificationStatus(customerId, isVerified, null);
        }

        public async Task UpdateVerificationStatus(int customerId, bool isVerified, string? reason)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer != null)
            {
                customer.IsVerified = isVerified;
                await _customerRepository.UpdateAsync(customer);
                await _auditLogManager.LogEventAsync("Customer.VerificationUpdated", "Customer", customerId.ToString(), $"Updated verification status to: {isVerified}", null, "Success");
                
                // 📨 Send Document Verification Email
                var user = await _userManager.FindByIdAsync(customer.aspNetUserId);
                if (user != null && !string.IsNullOrEmpty(user.Email)) {
                     var status = isVerified ? "Verified" : "Rejected/Unverified";
                     // If triggered by admin (assuming this method is called by admin action), 
                     // reason should be provided for rejection.
                     await _emailManager.SendDocumentVerificationEmail(user.Email, customer.Name, "Account/Documents", status, reason ?? "Administrative Decision", isVerified ? "You can now book cars." : "Please update your documents.");
                }
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
                
                // 📨 Send Account Status Email
                var user = await _userManager.FindByIdAsync(customer.aspNetUserId);
                if (user != null && !string.IsNullOrEmpty(user.Email)) {
                     var status = isActive ? "Activated" : "Deactivated";
                     await _emailManager.SendAccountStatusEmail(user.Email, customer.Name, status, "Administrative Action");
                }
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
            var customer = await _customerRepository.Query()
                .Include(c => c.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.User.Email == email);
            return _mapper.Map<CustomerDTO>(customer);
        }

        public async Task<CustomerDTO?> GetCustomerByUsername(string username)
        {
             var customer = await _customerRepository.Query()
                .Include(c => c.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.User.UserName == username);
            return _mapper.Map<CustomerDTO>(customer);
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
            await _auditLogManager.LogEventAsync("Customer.Deleted", "Customer", id.ToString(), $"Deleted customer account: {customerEntity.Name}", null, "Success");
        }


        public async Task UpdateCustomer(CustomerDTO dto)
        {
            _logger.LogInformation("Updating customer {Id}", dto.UserId);
            
            if (dto.UserId <= 0)
            {
                _logger.LogWarning("UpdateCustomer called with invalid UserId: {UserId}. Attempting to resolve via aspNetUserId.", dto.UserId);
                if (!string.IsNullOrEmpty(dto.aspNetUserId))
                {
                    var resolved = await _customerRepository.GetByIdAsync(dto.aspNetUserId);
                    if (resolved != null) dto.UserId = resolved.UserId;
                }
            }
            
            var customer = await _customerRepository.GetByIdAsync(dto.UserId);
            if (customer == null)
            {
                _logger.LogError("Customer with UserId {UserId} not found. Update aborted.", dto.UserId);
                return; // Or throw exception
            }
            
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

            var oldActive = customer.Isactive;
            var oldVerified = customer.IsVerified;

            // Capture Snapshot Before
            var before = new { 
                customer.Name, 
                customer.Address, 
                customer.IsVerified, 
                customer.Isactive,
                Email = user?.Email,
                Username = user?.UserName,
                PhoneNumber = user?.PhoneNumber
            };

            customer.Name = dto.Name;
            customer.Address = dto.Address;
            customer.IsVerified = dto.IsVerified;
            customer.Isactive = dto.Isactive;

            await _customerRepository.UpdateAsync(customer);
            _logger.LogInformation("Customer {Id} updated successfully in repository.", dto.UserId);

            // Capture Snapshot After
            var after = new { 
                customer.Name, 
                customer.Address, 
                customer.IsVerified, 
                customer.Isactive,
                Email = user?.Email,
                Username = user?.UserName,
                PhoneNumber = user?.PhoneNumber
            };

            await _auditLogManager.LogEventAsync(
                "Customer.ProfileUpdated", 
                "Customer", 
                dto.UserId.ToString(), 
                $"Updated profile details for: {customer.Name}", 
                null, 
                "Success",
                oldValues: before,
                newValues: after);

            // Check for Status Changes & Notify
            if (oldActive != customer.Isactive)
            {
                string status = customer.Isactive ? "Activated" : "Deactivated";
                string reason = customer.Isactive ? "Account has been reactivated by administrator." : "Account has been deactivated by administrator.";
                await _emailManager.SendAccountStatusEmail(user.Email, customer.Name, status, reason);
            }

            if (oldVerified != customer.IsVerified)
            {
                string status = customer.IsVerified ? "Verified" : "Unverified";
                string reason = customer.IsVerified ? "Your documents have been verified." : "Your verification status has been revoked.";
                 await _emailManager.SendDocumentVerificationEmail(user.Email, customer.Name, "Account Documents", status, reason, "");
            }
        }

        public async Task<bool> SendReminderToCustomerAsync(int customerId)
        {
             return await _emailManager.SendReminderToCustomerAsync(customerId);
        }

        public async Task<bool> ResetPassword(int customerId, string? specificPassword = null)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null) return false;
            var user = await _userManager.FindByIdAsync(customer.aspNetUserId);
            if (user == null) return false;
            
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var newPassword = !string.IsNullOrEmpty(specificPassword) 
                ? specificPassword 
                : $"RentCar{new Random().Next(100000, 999999)}!";
            
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            
            if (result.Succeeded)
            {
                 // 📨 Send Email with New Password
                 await _emailManager.SendAdminResetPasswordEmail(user.Email, newPassword, customer.Name);
                 await _auditLogManager.LogEventAsync("Customer.PasswordReset", "Customer", customerId.ToString(), "Admin reset customer password", null, "Success");
            }
            
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
        public async Task<IEnumerable<CustomerListDto>> GetAllCustomersForListAsync(string? search, bool? verified, bool? active)
        {
            var query = _customerRepository.Query()
                .Include(c => c.User)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                // We can't use .ToLower() translation in some providers properly if mixed, but usually fine in SQL
                // Better to use Case-insensitive collation or simple contains
                query = query.Where(c => c.Name.Contains(search) || c.User.Email.Contains(search) || c.UserId.ToString() == search);
            }

            if (verified.HasValue)
            {
                query = query.Where(c => c.IsVerified == verified.Value);
            }

            if (active.HasValue)
            {
                query = query.Where(c => c.Isactive == active.Value);
            }

            // Project directly to ListDto to avoid fetching BLOBs (Images)
            return await query.Select(c => new CustomerListDto
            {
                UserId = c.UserId,
                Name = c.Name,
                aspNetUserId = c.aspNetUserId,
                IsVerified = c.IsVerified,
                Isactive = c.Isactive,
                Address = c.Address,
                Email = c.User.Email,
                IsEmailConfirmed = c.User.EmailConfirmed,
                PhoneNumber = c.User.PhoneNumber
            }).ToListAsync();
        }

        public async Task<PagedResultDto<CustomerListDto>> GetCustomersPagedAsync(
            string? search, 
            bool? verified, 
            bool? active,
            int page = 1,
            int pageSize = 10,
            string? sortColumn = "Name",
            string? sortDirection = "asc")
        {
            var query = _customerRepository.Query()
                .Include(c => c.User)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(c => c.Name.Contains(search) || c.User.Email.Contains(search));
            }

            if (verified.HasValue)
            {
                query = query.Where(c => c.IsVerified == verified.Value);
            }

            if (active.HasValue)
            {
                query = query.Where(c => c.Isactive == active.Value);
            }

            var totalCount = await query.CountAsync();

            query = ApplySort(query, sortColumn, sortDirection);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerListDto
                {
                    UserId = c.UserId,
                    Name = c.Name,
                    aspNetUserId = c.aspNetUserId,
                    IsVerified = c.IsVerified,
                    Isactive = c.Isactive,
                    Address = c.Address,
                    Email = c.User.Email,
                    IsEmailConfirmed = c.User.EmailConfirmed,
                    PhoneNumber = c.User.PhoneNumber
                })
                .ToListAsync();

            return new PagedResultDto<CustomerListDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        private IQueryable<Customer> ApplySort(IQueryable<Customer> query, string? sortColumn, string? sortDirection)
        {
            return sortColumn?.ToLower() switch
            {
                "name" => sortDirection == "desc" ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "email" => sortDirection == "desc" ? query.OrderByDescending(c => c.User.Email) : query.OrderBy(c => c.User.Email),
                "isverified" => sortDirection == "desc" ? query.OrderByDescending(c => c.IsVerified) : query.OrderBy(c => c.IsVerified),
                "isactive" => sortDirection == "desc" ? query.OrderByDescending(c => c.Isactive) : query.OrderBy(c => c.Isactive),
                _ => query.OrderByDescending(c => c.UserId)
            };
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
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId)) // Explicit map
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore()); // skip reverse mapping User
                
            CreateMap<Customer, CustomerListDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.IsEmailConfirmed, opt => opt.MapFrom(src => src.User.EmailConfirmed));
        }
    }

}
