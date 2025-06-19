using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using System.Threading.Tasks;

namespace RentACar.Web.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class PaymentsModel : PageModel
    {
        private readonly CreditCardManager _cardManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CustomerManager _customerManager;

        public PaymentsModel(CreditCardManager cardManager,
            UserManager<IdentityUser> userManager,
            CustomerManager customerManager)
        {
            _cardManager = cardManager;
            _userManager = userManager;
            _customerManager = customerManager;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnGetAddFormAsync()
        {
            return Partial("_CardFormPartial", new CreditCardDto());
        }

        public async Task<IActionResult> OnGetEditFormAsync(int id)
        {
            var card = await _cardManager.GetCreditCardByIdAsync(id);
            if (card == null) return NotFound();
            return Partial("_CardFormPartial", card);
        }

        public async Task<IActionResult> OnGetDeleteFormAsync(int id)
        {
            var card = await _cardManager.GetCreditCardByIdAsync(id);
            if (card == null) return NotFound();
            return Partial("_DeleteCardPartial", card);
        }
    }
}
