using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs.Support;
using RentACar.Application.Managers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
        private readonly SupportManager _supportManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly BookingManager _bookingManager;
        private readonly RentACar.Core.Repositories.ICustomerRepository _customerRepository;

        public SupportController(
            SupportManager supportManager, 
            UserManager<IdentityUser> userManager, 
            BookingManager bookingManager,
            RentACar.Core.Repositories.ICustomerRepository customerRepository)
        {
            _supportManager = supportManager;
            _userManager = userManager;
            _bookingManager = bookingManager;
            _customerRepository = customerRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Load initial page of conversations
            var result = await _supportManager.GetCustomerConversationsPagedAsync(userId, 1, 10);
            
            // Get customer for booking history (needs int ID)
            var customer = await _customerRepository.GetByIdAsync(userId);
            if (customer != null)
            {
                var bookings = await _bookingManager.GetBookingHistoryAsync(customer.UserId);
                ViewBag.Bookings = bookings;
            }

            return View(result);
        }

        [HttpGet("Support/Conversation/{id}")]
        public async Task<IActionResult> Conversation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var conversation = await _supportManager.GetConversationDetailsForCustomerAsync(id, userId);
            if (conversation == null) return NotFound();

            return View(conversation);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSupportConversationDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (!ModelState.IsValid)
            {
                // For simplicity in MVC, if invalid redirect back to index
                return RedirectToAction(nameof(Index));
            }

            var conversationId = await _supportManager.CreateConversationAsync(userId, dto);
            return RedirectToAction(nameof(Conversation), new { id = conversationId });
        }
    }
}
