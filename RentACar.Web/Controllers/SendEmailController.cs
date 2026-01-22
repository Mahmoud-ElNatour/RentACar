using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using RentACar.Core.Entities;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [Route("Admin/EmailServices/SendEmail")]
    public class SendEmailController : Controller
    {
        private readonly EmailDraftManager _draftManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailManager _emailManager;
        private readonly EmailTemplateManager _templateManager;
        private readonly DistributionListManager _distListManager;

        public SendEmailController(
            EmailDraftManager draftManager,
            UserManager<IdentityUser> userManager,
            EmailManager emailManager,
            EmailTemplateManager templateManager,
            DistributionListManager distListManager)
        {
            _draftManager = draftManager;
            _userManager = userManager;
            _emailManager = emailManager;
            _templateManager = templateManager;
            _distListManager = distListManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
             var userId = _userManager.GetUserId(User);
             var drafts = await _draftManager.GetDraftsByUserAsync(userId);
             var templates = await _templateManager.GetAllTemplatesAsync();
             
             ViewBag.Drafts = drafts;
             ViewBag.Templates = templates.Where(t => t.IsActive).ToList();
             
             // Get list of distribution lists for the sidebar/modal
             ViewBag.DistributionLists = await _distListManager.GetAllListsAsync();

             return View("~/Views/EmailServices/SendEmail/Compose.cshtml");
        }

        [HttpPost("Send")]
        public async Task<IActionResult> Send([FromBody] SendEmailRequestDto request)
        {
             if (request == null) return BadRequest("Invalid request");

             // Resolve recipients
             var recipients = new HashSet<string>();

             // 1. Manual recipients
             if (!string.IsNullOrEmpty(request.RecipientsRaw))
             {
                 var manuals = request.RecipientsRaw.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                 foreach (var email in manuals)
                 {
                     if (new EmailAddressAttribute().IsValid(email.Trim()))
                     {
                         recipients.Add(email.Trim());
                     }
                 }
             }

             // 2. Distribution Lists
             if (!string.IsNullOrEmpty(request.SelectedDistributionListIdsRaw))
             {
                 var listIds = request.SelectedDistributionListIdsRaw
                     .Split(',', StringSplitOptions.RemoveEmptyEntries)
                     .Select(id => int.TryParse(id, out var i) ? i : 0)
                     .Where(i => i > 0)
                     .ToList();

                 foreach(var listId in listIds)
                 {
                      var listDto = await _distListManager.GetListByIdAsync(listId);
                      if (listDto != null && listDto.Members != null)
                      {
                          foreach(var member in listDto.Members)
                          {
                               if (!string.IsNullOrEmpty(member.Email))
                                   recipients.Add(member.Email);
                          }
                      }
                 }
             }

             if (!recipients.Any())
             {
                 return BadRequest("No valid recipients selected.");
             }

             // Process Attachments
             Dictionary<string, byte[]> attachments = null;
             if (request.Attachments != null && request.Attachments.Any())
             {
                 attachments = new Dictionary<string, byte[]>();
                 foreach (var file in request.Attachments)
                 {
                     if (file.Length > 0)
                     {
                         using (var ms = new System.IO.MemoryStream())
                         {
                             await file.CopyToAsync(ms);
                             attachments.Add(file.FileName, ms.ToArray());
                         }
                     }
                 }
             }

             // Send Logic
             string subject = request.Subject;
             string body = request.Body;
             int delivered;

             if (request.IsTemplateMode)
             {
                 // Template Mode: Use regular ad-hoc sender which might wrap logic, 
                 // BUT wait. If the user edited the body in template mode, that IS the raw HTML we want.
                 // "SendAdHocEmailBatchAsync" wraps it in "GetStandardTemplate". 
                 // If the body already contains the template container (div class="email-container"), wrapping it again is double wrapping.
                 // The editor shows the FULL HTML. 
                 // So we should probably use Raw send for everything IF the body is "complete".
                 // However, "Manual" mode definitely uses Raw send per request.
                 
                 // If TemplateKey is present and Body matches template default, we use Standard logic?
                 // Let's rely on IsTemplateMode. If IsTemplateMode is true, we assume it's "System Template" flow, 
                 // but if the Editor has the FULL content, we should just send it as Raw.
                 
                 // The safest bet given "SendRawEmailBatchAsync" exists is to use it if we want EXACT content.
                 // "SendAdHocEmailBatchAsync" WRAPS the content.
                 // If the user's template ALREADY has the wrapper (it does in the HTML snippet), we should NOT wrap it again.
                 
                 // So actually, both modes send RAW content if the editor contains the full HTML.
                 // "SendAdHoc" is only for simple strings.
                 
                 // Let's use SendRawEmailBatchAsync for everything coming from the Composer, as the Composer controls the full HTML.
                 delivered = await _emailManager.SendRawEmailBatchAsync(recipients, subject, body, attachments);
             }
             else
             {
                 // Manual Mode: Explicitly requested Raw.
                 delivered = await _emailManager.SendRawEmailBatchAsync(recipients, subject, body, attachments);
             }
             
             return Ok(new { success = true, recipientsCount = recipients.Count, deliveredCount = delivered });
        }

        [HttpPost("SaveDraft")]
        public async Task<IActionResult> SaveDraft([FromBody] SaveDraftRequestDto request)
        {
             var userId = _userManager.GetUserId(User);
             var draftId = await _draftManager.SaveDraftAsync(request, userId);
             return Ok(new { success = true, draftId });
        }

        [HttpGet("GetDraft/{id}")]
        public async Task<IActionResult> GetDraft(int id)
        {
             var userId = _userManager.GetUserId(User);
             var draft = await _draftManager.GetDraftAsync(id, userId);
             if (draft == null) return NotFound();
             return Ok(draft);
        }

        [HttpDelete("DeleteDraft/{id}")]
        public async Task<IActionResult> DeleteDraft(int id)
        {
             var userId = _userManager.GetUserId(User);
             await _draftManager.DeleteDraftAsync(id, userId);
             return Ok(new { success = true });
        }
    }
}
