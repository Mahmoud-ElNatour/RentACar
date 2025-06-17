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

        public MyCreditCardsController(CreditCardManager cardManager,
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
            if (user == null) return null;
            var customer = await _customerManager.GetCustomerByUsername(user.UserName);
            return customer?.UserId;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreditCardDto>>> Get()
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == null) return Unauthorized();
            var cards = await _cardManager.GetCardsForCustomerAsync(customerId.Value);
            return Ok(cards);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreditCardDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var created = await _cardManager.AddCreditCardAsync(dto, user.Email!, user.Id);
            if (created == null) return BadRequest(new { message = "Failed" });
            return Ok(new { message = "Done" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreditCardDto dto)
        {
            if (id != dto.CreditCardId) return BadRequest();
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var updated = await _cardManager.UpdateCreditCardAsync(dto, user.Email!, user.Id);
            if (updated == null) return BadRequest(new { message = "Failed" });
            return Ok(new { message = "Done" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == null) return Unauthorized();
            var result = await _cardManager.RemoveCustomerCardAsync(customerId.Value, id);
            if (!result) return BadRequest(new { message = "Failed" });
            return Ok(new { message = "Done" });
        }
    }
}
