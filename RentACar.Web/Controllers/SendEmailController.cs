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

             // Send Logic
             string subject = request.Subject;
             string body = request.Body;

             // If Template Mode, we might want to wrap the body or treat it differently?
             // But the user editor shows the "final" HTML or text.
             // For now, assume the frontend sends the *rendered* or *final* content in request.Body.
             // If manual, it's just the HTML.
             
             // UNLESS: The user just selected a template and didn't edit it? 
             // We need to support fetching the template if Body is empty but TemplateKey is set.
             if (request.IsTemplateMode && string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(request.TemplateKey))
             {
                  var template = await _templateManager.GetTemplateByKeyAsync(request.TemplateKey);
                  if (template != null)
                  {
                       subject = template.Subject; // Override or use provided? Usually provided subject overrides.
                       body = template.Body;
                  }
             }

             // ACTUALLY SEND
             int delivered = await _emailManager.SendAdHocEmailBatchAsync(recipients, subject, body);
             
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
