using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using AutoMapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Application.Managers
{
    public class PaymentMethodManager
    {
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<PaymentMethodManager> _logger;

        public PaymentMethodManager(
            IPaymentMethodRepository paymentMethodRepository,
            IMapper mapper,
            UserManager<IdentityUser> userManager,
            ILogger<PaymentMethodManager> logger)
        {
            _paymentMethodRepository = paymentMethodRepository;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<PaymentMethodDto?> AddPaymentMethodAsync(PaymentMethodDto dto, string userId)
        {
            _logger.LogInformation("Adding payment method {Name}", dto.PaymentMethodName);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                _logger.LogWarning("User {UserId} not authorized to add payment methods", userId);
                return null;
            }

            var existing = await _paymentMethodRepository.GetByNameAsync(dto.PaymentMethodName);
            if (existing != null)
            {
                _logger.LogWarning("Payment method name {Name} already exists", dto.PaymentMethodName);
                return null;
            }

            var entity = _mapper.Map<PaymentMethod>(dto);
            await _paymentMethodRepository.AddAsync(entity);
            return _mapper.Map<PaymentMethodDto>(entity);
        }

        public async Task<PaymentMethodDto?> GetPaymentMethodByIdAsync(int id)
        {
            var entity = await _paymentMethodRepository.GetByIdAsync(id);
            return _mapper.Map<PaymentMethodDto>(entity);
        }

        public async Task<PaymentMethodDto?> GetPaymentMethodByNameAsync(string name)
        {
            var entity = await _paymentMethodRepository.GetByNameAsync(name);
            return _mapper.Map<PaymentMethodDto>(entity);
        }

        public async Task<List<PaymentMethodDto>> GetAllPaymentMethodsAsync()
        {
            var list = await _paymentMethodRepository.GetAllAsync();
            return _mapper.Map<List<PaymentMethodDto>>(list);
        }

        public async Task<PaymentMethodDto?> UpdatePaymentMethodAsync(PaymentMethodDto dto, string userId)
        {
            _logger.LogInformation("Updating payment method {Id}", dto.Id);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                _logger.LogWarning("User {UserId} not authorized to update payment methods", userId);
                return null;
            }

            var existing = await _paymentMethodRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                _logger.LogWarning("Payment method {Id} not found", dto.Id);
                return null;
            }

            var methodWithName = await _paymentMethodRepository.GetByNameAsync(dto.PaymentMethodName);
            if (methodWithName != null && methodWithName.Id != dto.Id)
            {
                _logger.LogWarning("Payment method name {Name} already exists", dto.PaymentMethodName);
                return null;
            }

            _mapper.Map(dto, existing);
            await _paymentMethodRepository.UpdateAsync(existing);
            return _mapper.Map<PaymentMethodDto>(existing);
        }

        public async Task<bool> DeletePaymentMethodAsync(int id, string userId)
        {
            _logger.LogInformation("Deleting payment method {Id}", id);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                _logger.LogWarning("User {UserId} not authorized to delete payment method", userId);
                return false;
            }

            var existing = await _paymentMethodRepository.GetByIdAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Payment method {Id} not found", id);
                return false;
            }

            await _paymentMethodRepository.DeleteAsync(id);
            return true;
        }
    }

    // AutoMapper profile setup
    public class PaymentMethodProfile : Profile
    {
        public PaymentMethodProfile()
        {
            CreateMap<PaymentMethod, PaymentMethodDto>().ReverseMap();
        }
    }
}
