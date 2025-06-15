using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using AspNetUser = RentACar.Application.DTOs.AspNetUser;

namespace RentACar.Application.Managers
{
    public class CustomerManager
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;

        public CustomerManager(UserManager<IdentityUser> userManager,RoleManager<IdentityRole> roleManager,ICustomerRepository customerRepository,IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _customerRepository = customerRepository;
            _mapper = mapper;
        }
        public async Task<CustomerDTO?> CreateCustomer(CustomerCreateDTO createDto)
        {
            var user = new IdentityUser
            {
                UserName = createDto.Email,
                Email = createDto.Email,
                PhoneNumber = createDto.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, "C@c123456");
            if (!result.Succeeded)
            {
                // Log errors or handle them as needed
                return null;
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
            //Reset the Password

            return _mapper.Map<CustomerDTO>(customer);
        }

        public async Task<IdentityUser?> GetIdentityUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }


        public async Task<CustomerDTO?> GetCustomerById(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
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
            var customer = await GetCustomerById(id);
            if (customer == null)
                throw new Exception("Customer not found");

            var user = await _userManager.FindByIdAsync(customer.aspNetUserId);
            if (user == null)
                throw new Exception("User not found");

            await _customerRepository.DeleteAsync(id);
            await _userManager.DeleteAsync(user);
        }


        public async Task UpdateCustomer(CustomerDTO dto)
        {
            var customer = await _customerRepository.GetByIdAsync(dto.UserId);
            if (customer != null)
            {
                var user = await _userManager.FindByIdAsync(customer.aspNetUserId);
                if (user != null)
                {
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
            if (docs.DrivingLicenseFront != null && docs.DrivingLicenseBack != null)
            {
                await UpdateCustomerDrivingLicense(customerId, docs.DrivingLicenseFront, docs.DrivingLicenseBack);
            }

            if (docs.NationalIdfront != null && docs.NationalIdback != null)
            {
                await UpdateCustomerNationalId(customerId, docs.NationalIdfront, docs.NationalIdback);
            }
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
        }
    }

}