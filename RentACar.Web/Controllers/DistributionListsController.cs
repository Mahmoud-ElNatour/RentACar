using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
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
        public async Task<IActionResult> Details(int id)
        {
            var list = await _manager.GetListByIdAsync(id);
            if (list == null) return NotFound();
            return View("~/Views/EmailServices/DistributionLists/Details.cshtml", list);
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
            
            await _auditLogManager.LogAsync("Create Distribution List", "DistributionList", listId.ToString(), $"Created list '{dto.Name}'");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditList(DistributionListDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var userId = _userManager.GetUserId(User);
            await _manager.UpdateListAsync(dto, userId);

            await _auditLogManager.LogAsync("Update Distribution List", "DistributionList", dto.Id.ToString(), $"Updated list '{dto.Name}'");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteList(int id)
        {
            var userId = _userManager.GetUserId(User);
            await _manager.DeleteListAsync(id);
            
            await _auditLogManager.LogAsync("Delete Distribution List", "DistributionList", id.ToString(), $"Deleted distribution list");

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
            
            await _auditLogManager.LogAsync("Toggle Status", "DistributionList", id.ToString(), $"List '{list.Name}' was {status}");

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
            await _auditLogManager.LogAsync("Add Member", "DistributionListMember", listId.ToString(), $"Added {email} to '{list.Name}'");

            TempData["SuccessMessage"] = $"Member {email} added successfully.";
            return RedirectToAction(nameof(Details), new { id = listId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveMember(int memberId, int listId)
        {
            var userId = _userManager.GetUserId(User);
            var list = await _manager.GetListByIdAsync(listId); // Get list info before deleting member relation if possible, or just log ID
            
            await _manager.RemoveMemberAsync(memberId);
            
            await _auditLogManager.LogAsync("Remove Member", "DistributionListMember", memberId.ToString(), $"Removed member from list ID {listId}");

            TempData["SuccessMessage"] = "Member removed successfully.";
            return RedirectToAction(nameof(Details), new { id = listId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMember(int memberId, int listId, string email, string type, string label, bool isActive = false)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email is required");

            var userId = _userManager.GetUserId(User);
            
            // Get the member and update it
            var member = await _manager.GetMemberByIdAsync(memberId);
            if (member == null) return NotFound();

            member.Email = email;
            member.MemberType = type ?? "Other";
            member.Label = label;
            member.IsActive = isActive;

            await _manager.UpdateMemberAsync(member);

            await _auditLogManager.LogAsync("Update Member", "DistributionListMember", memberId.ToString(), $"Updated {email} in list ID {listId}");

            TempData["SuccessMessage"] = "Member updated successfully.";
            return RedirectToAction(nameof(Details), new { id = listId });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CalculateEstimatedRecipients([FromBody] DistributionListRuleDto rule)
        {
            var data = await _manager.PreviewRecipientsAsync(rule);
            var employees = data.Count(r => r.MemberType == "Employee");
            var customers = data.Count(r => r.MemberType == "Customer");
            var admins = data.Count(r => r.MemberType == "Admin");
            
            var breakdownParts = new System.Collections.Generic.List<string>();
            if (employees > 0) breakdownParts.Add($"{employees} Employees");
            if (customers > 0) breakdownParts.Add($"{customers} Customers");
            if (admins > 0) breakdownParts.Add($"{admins} Admins");
            
            var breakdown = breakdownParts.Count > 0 
                ? string.Join(" • ", breakdownParts)
                : "Select filters to calculate";
            
            return Json(new 
            { 
                totalCount = data.Count,
                breakdown = breakdown
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PreviewRecipients([FromBody] DistributionListRuleDto rule)
        {
            var data = await _manager.PreviewRecipientsAsync(rule);
            return PartialView("~/Views/EmailServices/DistributionLists/_PreviewRecipientsPartial.cshtml", data);
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

            await _auditLogManager.LogAsync("Create from Filters", "DistributionList", listId.ToString(), $"Created '{listName}' with {recipients.Count} members");

            return Ok(new { listId });
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int id)
        {
            var list = await _manager.GetListByIdAsync(id);
            if (list == null) return NotFound();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(list.Name);

            // Headers
            worksheet.Cell(1, 1).Value = "Email";
            worksheet.Cell(1, 2).Value = "Type";
            worksheet.Cell(1, 3).Value = "Label";
            worksheet.Cell(1, 4).Value = "Status";
            
            // Style headers
            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#daad52");
            headerRange.Style.Font.FontColor = XLColor.FromHtml("#13151b");

            // Data
            int row = 2;
            foreach (var member in list.Members)
            {
                worksheet.Cell(row, 1).Value = member.Email;
                worksheet.Cell(row, 2).Value = member.MemberType;
                worksheet.Cell(row, 3).Value = member.Label;
                worksheet.Cell(row, 4).Value = member.IsActive ? "Active" : "Inactive";
                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{list.Name}_Members.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var list = await _manager.GetListByIdAsync(id);
            if (list == null) return NotFound();
            return PartialView("~/Views/EmailServices/DistributionLists/_ConfirmDeletePartial.cshtml", list);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmToggle(int id)
        {
            var list = await _manager.GetListByIdAsync(id);
            if (list == null) return NotFound();
            return PartialView("~/Views/EmailServices/DistributionLists/_ConfirmTogglePartial.cshtml", list);
        }
    }
}
