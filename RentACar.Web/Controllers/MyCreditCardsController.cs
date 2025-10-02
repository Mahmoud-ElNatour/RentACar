using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    [ApiController]
    [Route("api/[controller]")]
    public class MyCreditCardsController : Controller
    {
        private readonly CreditCardManager _cardManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CustomerManager _customerManager;
        private readonly ILogger<MyCreditCardsController> _logger;

        public MyCreditCardsController(
            CreditCardManager cardManager,
            UserManager<IdentityUser> userManager,
            CustomerManager customerManager,
            ILogger<MyCreditCardsController> logger)
        {
            _cardManager = cardManager;
            _userManager = userManager;
            _customerManager = customerManager;
            _logger = logger;
        }

        private async Task<int?> GetCurrentCustomerId()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("GetCurrentCustomerId: No authenticated user found.");
                return null;
            }

            var customer = await _customerManager.GetCustomerByUsername(user.UserName);
            if (customer == null)
            {
                _logger.LogWarning("GetCurrentCustomerId: No customer found for user {Username}", user.UserName);
            }

            return customer?.UserId;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreditCardDto>>> Get()
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to Get credit cards.");
                return Unauthorized();
            }

            var cards = await _cardManager.GetCardsForCustomerAsync(customerId.Value);
            _logger.LogInformation("Fetched {Count} credit cards for customer {CustomerId}", cards.Count, customerId);
            return Ok(cards);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreditCardDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Unauthorized POST attempt to add credit card.");
                return Unauthorized();
            }

            _logger.LogInformation("Attempting to add credit card for user {UserId}", user.Id);
            var created = await _cardManager.AddCreditCardAsync(dto, user.Email!, user.Id);
            if (created == null)
            {
                _logger.LogError("Failed to add credit card for user {UserId}", user.Id);
                return BadRequest(new { message = "Failed" });
            }

            _logger.LogInformation("Successfully added credit card for user {UserId}", user.Id);
            return Ok(new { message = "Done" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreditCardDto dto)
        {
            if (id != dto.CreditCardId)
            {
                _logger.LogWarning("Mismatched ID in PUT request. Route ID: {RouteId}, DTO ID: {DtoId}", id, dto.CreditCardId);
                return BadRequest();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Unauthorized PUT attempt to update credit card.");
                return Unauthorized();
            }

            _logger.LogInformation("Attempting to update credit card {CardId} for user {UserId}", id, user.Id);
            var updated = await _cardManager.UpdateCreditCardAsync(dto, user.Email!, user.Id);
            if (updated == null)
            {
                _logger.LogError("Failed to update credit card {CardId} for user {UserId}", id, user.Id);
                return BadRequest(new { message = "Failed" });
            }

            _logger.LogInformation("Successfully updated credit card {CardId} for user {UserId}", id, user.Id);
            return Ok(new { message = "Done" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == null)
            {
                _logger.LogWarning("Unauthorized DELETE attempt for credit card {CardId}.", id);
                return Unauthorized();
            }

            _logger.LogInformation("Attempting to delete credit card {CardId} for customer {CustomerId}", id, customerId);
            var result = await _cardManager.RemoveCustomerCardAsync(customerId.Value, id);
            if (!result)
            {
                _logger.LogError("Failed to delete credit card {CardId} for customer {CustomerId}", id, customerId);
                return BadRequest(new { message = "Failed" });
            }

            _logger.LogInformation("Successfully deleted credit card {CardId} for customer {CustomerId}", id, customerId);
            return Ok(new { message = "Done" });
        }
    }
}
