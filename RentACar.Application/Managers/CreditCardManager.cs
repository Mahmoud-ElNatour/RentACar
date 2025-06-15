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
using AspNetUser = RentACar.Application.DTOs.AspNetUser;

namespace RentACar.Core.Managers
{
    public class CreditCardManager
    {
        private readonly ICreditCardRepository _creditCardRepository; // Repository for accessing credit card data
        private readonly IMapper _mapper; // AutoMapper instance for object-to-object mapping
        private readonly UserManager<AspNetUser> _userManager; // ASP.NET Identity UserManager for user-related operations
        private readonly CustomerManager _customerManager; // Manager for customer-related business logic

        // Constructor to inject dependencies
        public CreditCardManager(
            ICreditCardRepository creditCardRepository,
            IMapper mapper,
            UserManager<AspNetUser> userManager,
            CustomerManager customerManager)
        {
            _creditCardRepository = creditCardRepository;
            _mapper = mapper;
            _userManager = userManager;
            _customerManager = customerManager;
        }

        // Allows a logged-in customer to add a credit card to their account
        public async Task<CreditCardDto?> AddCreditCardAsync(CreditCardDto creditCardDto, Customer loggedInCustomer)
        {
            // Check if the logged-in customer object is not null
            if (loggedInCustomer == null)
                return null; // Indicate failure if no customer is provided

            // Call the AddOrLinkCreditCard method to handle the actual logic
            return await AddOrLinkCreditCard(creditCardDto, loggedInCustomer.UserId);
        }

        // Allows a logged-in employee to add a credit card to a specific customer's account
        public async Task<CreditCardDto?> AddCreditCardAsync(CreditCardDto creditCardDto, Employee loggedInEmployee, int customerUserId)
        {
            // Check if the logged-in employee object is not null
            if (loggedInEmployee == null)
                return null; // Indicate failure if no employee is provided

            // Call the AddOrLinkCreditCard method to handle the actual logic
            return await AddOrLinkCreditCard(creditCardDto, customerUserId);
        }

        // Private helper method to either add a new credit card or link an existing one to a user
        private async Task<CreditCardDto?> AddOrLinkCreditCard(CreditCardDto creditCardDto, int userId)
        {
            // Validate the format of the credit card number using the Luhn algorithm
            if (!IsValidCardNumber(creditCardDto.CardNumber))
                return null; // Indicate failure if the card number is invalid

            // Validate if the expiration date is in the future
            if (creditCardDto.ExpiryDate.ToDateTime(TimeOnly.MinValue) <= DateTime.UtcNow)
                return null; // Indicate failure if the card is expired

            // Check if a credit card with the given number already exists in the database
            var existingCard = await _creditCardRepository.GetByCardNumberAsync(creditCardDto.CardNumber);
            CreditCard creditCardEntity;

            // If the credit card doesn't exist, create a new one
            if (existingCard == null)
            {
                // Map the CreditCardDto to the CreditCard entity
                creditCardEntity = _mapper.Map<CreditCard>(creditCardDto);
                // Add the new credit card to the database and get the added entity
                var addedEntity = await _creditCardRepository.AddAsync(creditCardEntity);
                // Link the new credit card to the customer's account in the CustomerCreditCard table
                await _creditCardRepository.AddCustomerCreditCardAsync(new CustomerCreditCard
                {
                    UserId = userId,
                    CreditCardId = addedEntity.CreditCardId
                });
                // Map the added CreditCard entity back to a CreditCardDto and return it
                return _mapper.Map<CreditCardDto>(addedEntity);
            }
            // If the credit card already exists, link it to the customer's account if it's not already linked
            else
            {
                // Get all credit cards associated with the user
                var customerCards = await _creditCardRepository.GetCustomerCreditCardsAsync(userId);
                // Check if the existing card is not already linked to the user
                if (!customerCards.Any(c => c.CreditCardId == existingCard.CreditCardId))
                {
                    // Link the existing credit card to the customer's account
                    await _creditCardRepository.AddCustomerCreditCardAsync(new CustomerCreditCard
                    {
                        UserId = userId,
                        CreditCardId = existingCard.CreditCardId
                    });
                }
                // Map the existing CreditCard entity to a CreditCardDto and return it
                return _mapper.Map<CreditCardDto>(existingCard);
            }
        }

        // Retrieves a credit card by its ID
        public async Task<CreditCardDto?> GetCreditCardByIdAsync(int id)
        {
            var creditCard = await _creditCardRepository.GetByIdAsync(id);
            return _mapper.Map<CreditCardDto>(creditCard);
        }

        // Retrieves all credit cards associated with a specific user ID
        public async Task<List<CreditCardDto>> GetCustomerCreditCardsAsync(int userId)
        {
            var creditCards = await _creditCardRepository.GetCustomerCreditCardsAsync(userId);
            return _mapper.Map<List<CreditCardDto>>(creditCards);
        }

        // Updates an existing credit card
        public async Task<CreditCardDto?> UpdateCreditCardAsync(CreditCardDto creditCardDto, int userId)
        {
            // Retrieve the user and verify role in one shot
            var customer =await _customerManager.GetCustomerById(userId);
            var user = await _userManager.FindByIdAsync(customer.aspNetUserId);
            var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");
            var isCustomer = user != null && await _userManager.IsInRoleAsync(user, "Customer");

            if (user == null || (!isAdmin && !isCustomer))
                return null;

            // Get the credit card by ID
            var existingCard = await _creditCardRepository.GetByIdAsync(creditCardDto.CreditCardId);
            if (existingCard == null)
                return null;

            // Authorization: Ensure customer owns the card unless they're an admin
            if (!isAdmin)
            {
                var customerCards = await _creditCardRepository.GetCustomerCreditCardsAsync(userId);
                if (!customerCards.Any(c => c.CreditCardId == creditCardDto.CreditCardId))
                    return null;
            }

            // Map updates and save
            _mapper.Map(creditCardDto, existingCard);
            await _creditCardRepository.UpdateAsync(existingCard);

            return _mapper.Map<CreditCardDto>(existingCard);
        }

        // Removes a credit card from a customer's account (does not delete the credit card record entirely)
        public async Task<bool> DeleteCreditCardFromCustomerAsync(int creditCardId, int userId, string loggedInUserId)
        {
            // Retrieve the user based on the provided logged-in user ID
            var customer = await _customerManager.GetCustomerById(userId);
            var user = await _userManager.FindByIdAsync(loggedInUserId);
            // Check if the user exists and has either "Customer" or "Admin" role
            if (user == null || (!await _userManager.IsInRoleAsync(user, "Customer") && !await _userManager.IsInRoleAsync(user, "Admin")))
                return false; // Return false if the user is not authorized

            // Prevent non-admin users from deleting other users' credit cards
            if (customer.aspNetUserId != loggedInUserId && !await _userManager.IsInRoleAsync(user, "Admin"))
                return false; // Return false if the user is trying to delete another user's card

            // Remove the link between the user and the credit card
            await _creditCardRepository.RemoveCreditCardAsync(userId, creditCardId);
            return true; // Return true if the operation was successful
        }

        // Deletes a credit card record entirely from the system (admin only)
        public async Task<bool> DeleteCreditCardEntirelyAsync(int id, string userId)
        {
            // Retrieve the user based on the provided ID
            var user = await _userManager.FindByIdAsync(userId);
            // Check if the user exists and has the "Admin" role
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
                return false; // Return false if the user is not an admin

            // Retrieve the existing credit card from the database
            var existingCard = await _creditCardRepository.GetByIdAsync(id);
            if (existingCard == null)
                return false; // Return false if the credit card does not exist

            // Delete the credit card record from the database
            await _creditCardRepository.DeleteAsync(existingCard);
            return true; // Return true if the operation was successful
        }

        // Retrieves a credit card by its card number
        public async Task<CreditCardDto?> GetCreditCardByCardNumberAsync(string cardNumber)
        {
            var creditCard = await _creditCardRepository.GetByCardNumberAsync(cardNumber);
            return _mapper.Map<CreditCardDto>(creditCard);
        }

        // Retrieves all credit cards in the system (admin only)
        public async Task<List<CreditCardDto>> GetAllCreditCardsAsync(string userId)
        {
            // Retrieve the user based on the provided ID
            var user = await _userManager.FindByIdAsync(userId);
            // Check if the user exists and has the "Admin" role
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
                return new List<CreditCardDto>(); // Return an empty list if the user is not an admin

            // Retrieve all credit cards from the database
            var creditCards = await _creditCardRepository.GetAllAsync();
            return _mapper.Map<List<CreditCardDto>>(creditCards);
        }

        public async Task<(List<CreditCardDisplayDto> Cards, int Total)> SearchCreditCardsAsync(string userId, string? cardNumber, string? customer, int offset, int limit = 30)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var isAllowed = user != null && (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "Employee"));
            if (!isAllowed)
                return (new List<CreditCardDisplayDto>(), 0);

            var (cards, total) = await _creditCardRepository.SearchAsync(cardNumber, customer, offset, limit);
            var result = new List<CreditCardDisplayDto>();
            foreach (var c in cards)
            {
                var cc = c.CustomerCreditCards.FirstOrDefault();
                result.Add(new CreditCardDisplayDto
                {
                    CreditCardId = c.CreditCardId,
                    CardNumber = c.CardNumber,
                    CardHolderName = c.CardHolderName,
                    ExpiryDate = c.ExpiryDate,
                    CustomerName = cc?.User.Name ?? string.Empty,
                    CustomerEmail = cc?.User.User.Email ?? string.Empty,
                    CustomerId = cc?.User.UserId ?? 0
                });
            }
            return (result, total);
        }

        // Luhn Algorithm: Validates the format of a credit card number
        private bool IsValidCardNumber(string cardNumber)
        {
            // Remove any non-digit characters from the card number
            cardNumber = new string(cardNumber.Where(char.IsDigit).ToArray());
            int sum = 0;
            bool alternate = false;

            // Iterate through the card number from right to left
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                // Parse the current digit
                int n = int.Parse(cardNumber[i].ToString());
                // For every second digit, double it
                if (alternate)
                {
                    n *= 2;
                    // If the doubled value is greater than 9, subtract 9
                    if (n > 9) n -= 9;
                }
                // Add the digit (or the processed doubled digit) to the sum
                sum += n;
                // Toggle the alternate flag
                alternate = !alternate;
            }

            // The card number is valid if the sum is divisible by 10
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