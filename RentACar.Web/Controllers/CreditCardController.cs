using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Core.Managers;
using System.Linq;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [ApiController]
    [Route("api/[controller]")]
    public class CreditCardController : Controller
    {
        private readonly CreditCardManager _creditCardManager;
        private readonly UserManager<IdentityUser> _userManager;

        public CreditCardController(CreditCardManager creditCardManager, UserManager<IdentityUser> userManager)
        {
            _creditCardManager = creditCardManager;
            _userManager = userManager;
        }

        [HttpGet("~/CreditCard")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View("~/Views/ControlPanel/CreditCard/Index.cshtml");
        }

        [HttpGet("~/CreditCard/Add")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult AddForm()
        {
            return PartialView("~/Views/ControlPanel/CreditCard/_CreditCardFormPartial.cshtml", new CreditCardDisplayDto());
        }

        [HttpGet("~/CreditCard/Edit/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> EditForm(int id)
        {
            var card = await _creditCardManager.GetCreditCardByIdAsync(id);
            if (card == null) return NotFound();
            // find customer id
            var (cards, _) = await _creditCardManager.GetCardsAsync(card.CardNumber, null, 0, 1);
            var display = cards.FirstOrDefault();
            var dto = new CreditCardDisplayDto
            {
                CreditCardId = card.CreditCardId,
                CardNumber = card.CardNumber,
                CardHolderName = card.CardHolderName,
                ExpiryDate = card.ExpiryDate,
                Cvv = card.Cvv,
                CustomerId = display?.CustomerId ?? 0,
                CustomerName = display?.CustomerName ?? string.Empty,
                CustomerEmail = display?.CustomerEmail ?? string.Empty
            };
            return PartialView("~/Views/ControlPanel/CreditCard/_CreditCardFormPartial.cshtml", dto);
        }

        [HttpGet("~/CreditCard/Delete/{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var card = await _creditCardManager.GetCreditCardByIdAsync(id);
            if (card == null) return NotFound();
            return PartialView("~/Views/ControlPanel/CreditCard/_DeleteCreditCardPartial.cshtml", card);
        }

        [HttpGet]
        public async Task<ActionResult<object>> Get([FromQuery] string? cardNumber, [FromQuery] string? customer, [FromQuery] int? offset)
        {
            var result = await _creditCardManager.GetCardsAsync(cardNumber, customer, offset ?? 0);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreditCardDisplayDto dto)
        {
            var userId = _userManager.GetUserId(User)!;
            var cardDto = new CreditCardDto
            {
                CardNumber = dto.CardNumber,
                CardHolderName = dto.CardHolderName,
                ExpiryDate = dto.ExpiryDate,
                Cvv = dto.Cvv
            };
            var created = await _creditCardManager.AddCreditCardAsync(cardDto, dto.CustomerEmail, userId);
            if (created == null) return BadRequest(new { message = "Failed" });
            return Ok(new { message = "Done" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreditCardDisplayDto dto)
        {
            if (id != dto.CreditCardId) return BadRequest();
            var userId = _userManager.GetUserId(User)!;
            var updated = await _creditCardManager.UpdateCreditCardAsync(new CreditCardDto
            {
                CreditCardId = dto.CreditCardId,
                CardNumber = dto.CardNumber,
                CardHolderName = dto.CardHolderName,
                ExpiryDate = dto.ExpiryDate,
                Cvv = dto.Cvv
            }, dto.CustomerEmail, userId);
            if (updated == null) return BadRequest(new { message = "Failed" });
            return Ok(new { message = "Done" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var result = await _creditCardManager.DeleteCreditCardAsync(id, userId);
            if (!result) return BadRequest(new { message = "Failed" });
            return Ok(new { message = "Done" });
        }

    }
}
