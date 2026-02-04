using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs.Support;
using RentACar.Application.Managers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RentACar.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class SupportInboxController : Controller
    {
        private readonly SupportManager _supportManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<SupportInboxController> _logger;

        public SupportInboxController(SupportManager supportManager, UserManager<IdentityUser> userManager, ILogger<SupportInboxController> logger)
        {
            _supportManager = supportManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? status = null, string? category = null, string? searchQuery = null, string? assignedEmployeeId = null)
        {
            var result = await _supportManager.GetAllConversationsPagedAsync(page, pageSize, status, category, searchQuery, assignedEmployeeId);
            var stats = await _supportManager.GetSupportStatsAsync();
            
            // Load all staff for the "Assigned To" dropdown
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var allStaff = employees.Concat(admins).Distinct().OrderBy(u => u.UserName).ToList();

            ViewBag.Stats = stats;
            ViewBag.Status = status;
            ViewBag.Category = category;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.AssignedEmployeeId = assignedEmployeeId;
            ViewBag.Staff = allStaff;

            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var conversation = await _supportManager.GetConversationDetailsForEmployeeAsync(id, userId);
            if (conversation == null) return NotFound();

            // Load all employees for assignment dropdown
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var allStaff = employees.Concat(admins).Distinct().ToList();
            
            ViewBag.Staff = allStaff;

            return View(conversation);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(UpdateSupportConversationStatusDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            bool success = await _supportManager.UpdateStatusAsync(userId, dto.ConversationId, dto.NewStatus);
            
            if (success)
            {
                _logger.LogInformation("Support ticket {ConversationId} resolved successfully by user {UserId}", dto.ConversationId, userId);
                TempData["SupportActionSuccess"] = "Ticket has been resolved successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
               _logger.LogWarning("Failed to resolve support ticket {ConversationId} by user {UserId}", dto.ConversationId, userId);
               TempData["SupportActionError"] = "Failed to resolve ticket. Please try again.";
               return RedirectToAction(nameof(Details), new { id = dto.ConversationId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reassign(int conversationId, string targetEmployeeId, string note)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _supportManager.ReassignAsync(userId, conversationId, targetEmployeeId, note);
            return RedirectToAction(nameof(Details), new { id = conversationId });
        }

        [HttpGet]
        public async Task<IActionResult> SearchStaff(string term)
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var allStaff = employees.Concat(admins).Distinct()
                .Where(u => (u.UserName != null && u.UserName.Contains(term, System.StringComparison.OrdinalIgnoreCase)) || 
                            (u.Email != null && u.Email.Contains(term, System.StringComparison.OrdinalIgnoreCase)))
                .Select(u => new { id = u.Id, text = u.UserName ?? u.Email })
                .ToList();

            return Json(allStaff);
        }

        [HttpPost]
        public async Task<IActionResult> AddInternalNote(int conversationId, string note)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _supportManager.AddInternalNoteAsync(userId, conversationId, note);
            return RedirectToAction(nameof(Details), new { id = conversationId });
        }
    }
}
