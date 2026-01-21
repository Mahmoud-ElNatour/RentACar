using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Infrastructure.Data;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class DistributionListsController : Controller
    {
        private readonly DistributionListManager _manager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AuditLogManager _auditLogManager;

        public DistributionListsController(
            DistributionListManager manager, 
            UserManager<IdentityUser> userManager,
            AuditLogManager auditLogManager)
        {
            _manager = manager;
            _userManager = userManager;
            _auditLogManager = auditLogManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var lists = await _manager.GetAllListsAsync();
            return View("~/Views/EmailServices/DistributionLists/Index.cshtml", lists);
        }

        [HttpGet]
        public async Task<IActionResult> GetListMembers(int id)
        {
            var list = await _manager.GetListByIdAsync(id);
            if (list == null) return NotFound();
            return PartialView("~/Views/EmailServices/DistributionLists/_ListMembersModalOrPagePartial.cshtml", list);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateList(DistributionListDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = _userManager.GetUserId(User);
            var listId = await _manager.CreateListAsync(dto, userId);
            
            await _auditLogManager.LogAsync(userId, "Create Distribution List", $"Created list '{dto.Name}' (ID: {listId})", "DistributionList", listId.ToString());

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditList(DistributionListDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var userId = _userManager.GetUserId(User);
            await _manager.UpdateListAsync(dto, userId);

            await _auditLogManager.LogAsync(userId, "Edit Distribution List", $"Updated list '{dto.Name}' (ID: {dto.Id})", "DistributionList", dto.Id.ToString());

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteList(int id)
        {
            var userId = _userManager.GetUserId(User);
            await _manager.DeleteListAsync(id);
            
            await _auditLogManager.LogAsync(userId, "Delete Distribution List", $"Deleted list ID: {id}", "DistributionList", id.ToString());

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            if (!User.IsInRole("Admin")) return Forbid();

            var userId = _userManager.GetUserId(User);
            await _manager.ToggleListActiveAsync(id, userId);

            var list = await _manager.GetListByIdAsync(id);
            var status = list.IsActive ? "activated" : "deactivated";
            
            await _auditLogManager.LogAsync(userId, "Toggle Distribution List", $"List '{list.Name}' was {status}", "DistributionList", id.ToString());

            TempData["SuccessMessage"] = $"Distribution List '{list.Name}' has been {status}.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddMember(int listId, string email, string label, string type)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email is required");

            var userId = _userManager.GetUserId(User);
            await _manager.AddMemberAsync(listId, email, label ?? "Manual", type ?? "Other", userId);

            var list = await _manager.GetListByIdAsync(listId);
            await _auditLogManager.LogAsync(userId, "Add List Member", $"Added member {email} to list '{list.Name}'", "DistributionListMember", listId.ToString());

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveMember(int memberId, int listId)
        {
            var userId = _userManager.GetUserId(User);
            var list = await _manager.GetListByIdAsync(listId); // Get list info before deleting member relation if possible, or just log ID
            
            await _manager.RemoveMemberAsync(memberId);
            
            await _auditLogManager.LogAsync(userId, "Remove List Member", $"Removed member ID {memberId} from list ID {listId}", "DistributionListMember", memberId.ToString());

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> PreviewRecipients([FromBody] DistributionListRuleDto rule)
        {
            var data = await _manager.PreviewRecipientsAsync(rule);
            return Ok(new { count = data.Count, recipients = data }); 
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateFromGenerated([FromBody] DistributionListRuleDto rule, [FromQuery] string listName)
        {
            var recipients = await _manager.PreviewRecipientsAsync(rule);
            
            var userId = _userManager.GetUserId(User);
            
            var listDto = new DistributionListDto
            {
                Name = listName,
                IsActive = true,
                Description = "Generated from Filters"
            };

            var listId = await _manager.CreateListAsync(listDto, userId);

            foreach (var r in recipients)
            {
                await _manager.AddMemberAsync(listId, r.Email, r.Label, r.MemberType, userId);
            }

            await _auditLogManager.LogAsync(userId, "Create Generated List", $"Created list '{listName}' with {recipients.Count} members from filters", "DistributionList", listId.ToString());

            return Ok(new { listId });
        }
    }
}
