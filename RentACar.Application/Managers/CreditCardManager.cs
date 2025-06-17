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

namespace RentACar.Application.Managers
{
    public class CreditCardManager
    {
        private readonly ICreditCardRepository _repo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CustomerManager _customerManager;
        private readonly IMapper _mapper;
        private readonly ILogger<CreditCardManager> _logger;

        public CreditCardManager(ICreditCardRepository repo,
                                 UserManager<IdentityUser> userManager,
                                 CustomerManager customerManager,
                                 IMapper mapper,
                                 ILogger<CreditCardManager> logger)
        {
            _repo = repo;
            _userManager = userManager;
            _customerManager = customerManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<(List<CreditCardDisplayDto> Cards, int Total)> GetCardsAsync(string? cardNumber, string? customer, int offset, int limit = 30)
        {
            var (cards, total) = await _repo.SearchAsync(cardNumber, customer, offset, limit);
            var list = new List<CreditCardDisplayDto>();
            foreach (var c in cards)
            {
                var cc = c.CustomerCreditCards.FirstOrDefault();
                list.Add(new CreditCardDisplayDto
                {
                    CreditCardId = c.CreditCardId,
                    CardNumber = c.CardNumber,
                    CardHolderName = c.CardHolderName,
                    ExpiryDate = c.ExpiryDate,
                    Cvv = c.Cvv,
                    CustomerName = cc?.User.Name ?? string.Empty,
                    CustomerEmail = cc?.User.User.Email ?? string.Empty,
                    CustomerId = cc?.User.UserId ?? 0
                });
            }
            return (list, total);
        }

        public async Task<CreditCardDto?> GetCreditCardByIdAsync(int id)
        {
            var card = await _repo.GetByIdAsync(id);
            return _mapper.Map<CreditCardDto>(card);
        }

        public async Task<CreditCardDto?> AddCreditCardAsync(CreditCardDto dto, string customerEmail, string operatorUserId)
        {
            _logger.LogInformation("Adding credit card for customer {Email}", customerEmail);
            var opUser = await _userManager.FindByIdAsync(operatorUserId);
            if (opUser == null) return null;

            var customer = await _customerManager.GetCustomerByEmail(customerEmail);
            if (customer == null) return null;

            if (!await CanOperateAsync(opUser, customer))
                return null;

            return await AddOrLink(dto, customer.UserId);
        }

        public async Task<CreditCardDto?> UpdateCreditCardAsync(CreditCardDto dto, string customerEmail, string operatorUserId)
        {
            _logger.LogInformation("Updating credit card {Id}", dto.CreditCardId);
            var opUser = await _userManager.FindByIdAsync(operatorUserId);
            if (opUser == null) return null;

            var customer = await _customerManager.GetCustomerByEmail(customerEmail);
            if (customer == null) return null;

            if (!await CanOperateAsync(opUser, customer))
                return null;

            var existing = await _repo.GetByIdAsync(dto.CreditCardId);
            if (existing == null) return null;

            if (!await _userManager.IsInRoleAsync(opUser, "Admin") &&
                !await _userManager.IsInRoleAsync(opUser, "Employee"))
            {
                var custCards = await _repo.GetCustomerCreditCardsAsync(customer.UserId);
                if (!custCards.Any(c => c.CreditCardId == dto.CreditCardId))
                    return null;
            }

            _mapper.Map(dto, existing);
            await _repo.UpdateAsync(existing);
            _logger.LogInformation("Credit card {Id} updated", dto.CreditCardId);
            return _mapper.Map<CreditCardDto>(existing);
        }

        public async Task<bool> DeleteCreditCardAsync(int id, string operatorUserId)
        {
            _logger.LogInformation("Deleting credit card {Id}", id);
            var user = await _userManager.FindByIdAsync(operatorUserId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
                return false;

            var card = await _repo.GetByIdAsync(id);
            if (card == null) return false;

            await _repo.DeleteAsync(card);
            return true;
        }

        private async Task<bool> CanOperateAsync(IdentityUser operatorUser, CustomerDTO customer)
        {
            if (await _userManager.IsInRoleAsync(operatorUser, "Admin") ||
                await _userManager.IsInRoleAsync(operatorUser, "Employee"))
                return true;

            if (await _userManager.IsInRoleAsync(operatorUser, "Customer"))
                return customer.aspNetUserId == operatorUser.Id;

            return false;
        }

        private async Task<CreditCardDto?> AddOrLink(CreditCardDto dto, int userId)
        {
            if (!IsValidCardNumber(dto.CardNumber) ||
                dto.ExpiryDate.ToDateTime(TimeOnly.MinValue) <= DateTime.UtcNow)
                return null;

            var existing = await _repo.GetByCardNumberAsync(dto.CardNumber);
            CreditCard entity;
            if (existing == null)
            {
                entity = _mapper.Map<CreditCard>(dto);
                entity = await _repo.AddAsync(entity);
                await _repo.AddCustomerCreditCardAsync(new CustomerCreditCard { UserId = userId, CreditCardId = entity.CreditCardId });
            }
            else
            {
                entity = existing;
                var list = await _repo.GetCustomerCreditCardsAsync(userId);
                if (!list.Any(c => c.CreditCardId == existing.CreditCardId))
                    await _repo.AddCustomerCreditCardAsync(new CustomerCreditCard { UserId = userId, CreditCardId = existing.CreditCardId });
            }
            return _mapper.Map<CreditCardDto>(entity);
        }

        private bool IsValidCardNumber(string cardNumber)
        {
            cardNumber = new string(cardNumber.Where(char.IsDigit).ToArray());
            int sum = 0; bool alt = false;
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int n = int.Parse(cardNumber[i].ToString());
                if (alt)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }
                sum += n;
                alt = !alt;
            }
            return sum % 10 == 0;
        }
    }

    public class CreditCardProfile : Profile
    {
        public CreditCardProfile()
        {
            CreateMap<CreditCard, CreditCardDto>().ReverseMap();
        }
    }
}
