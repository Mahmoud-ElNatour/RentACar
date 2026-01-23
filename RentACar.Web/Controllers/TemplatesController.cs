using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.Managers;
using RentACar.Core.Entities;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [Route("Admin/EmailServices/Templates")]
    public class TemplatesController : Controller
    {
        private readonly EmailTemplateManager _templateManager;
        private readonly UserManager<IdentityUser> _userManager;

        public TemplatesController(EmailTemplateManager templateManager, UserManager<IdentityUser> userManager)
        {
            _templateManager = templateManager;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var templates = await _templateManager.GetAllTemplatesAsync();
            return View("~/Views/EmailServices/Templates/Index.cshtml", templates);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/EmailServices/Templates/Create.cshtml", new EmailTemplate());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmailTemplate template)
        {
            // Remove error for UpdatedByUser as it's set in manager
            ModelState.Remove(nameof(template.UpdatedByUser));
            ModelState.Remove(nameof(template.UpdatedByUserId));

            if (!ModelState.IsValid)
            {
                return View("~/Views/EmailServices/Templates/Create.cshtml", template);
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                 // Normalize key
                template.TemplateKey = template.TemplateKey?.Trim().Replace(" ", "_");
                
                await _templateManager.CreateTemplateAsync(template, userId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/EmailServices/Templates/Create.cshtml", template);
            }
        }

        // Changed to use Key
        [HttpGet("Edit/{key}")]
        public async Task<IActionResult> Edit(string key)
        {
            if (string.IsNullOrEmpty(key)) return NotFound();

            var template = await _templateManager.GetTemplateByKeyAsync(key);
            if (template == null) return NotFound();

            return View("~/Views/EmailServices/Templates/Edit.cshtml", template);
        }

        [HttpPost("Edit/{key}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string key, EmailTemplate template)
        {
             if (key != template.TemplateKey) return BadRequest();
             
             try 
             {
                 var userId = _userManager.GetUserId(User);
                 await _templateManager.UpdateTemplateAsync(template, userId);
                 return RedirectToAction("Index");
             }
             catch(Exception ex)
             {
                 ModelState.AddModelError("", ex.Message);
                 return View("~/Views/EmailServices/Templates/Edit.cshtml", template);
             }
        }

        // Removed Preview (handled in Edit view via JS)

        [HttpPost("Delete/{key}")]
        public async Task<IActionResult> Delete(string key)
        {
            await _templateManager.DeleteTemplateByKeyAsync(key);
            return Ok(new { success = true });
        }

        // Assuming ToggleActive might be called by ID still or Key. 
        // Index view doesn't seem to implement ToggleActive in the new design provided, 
        // but let's keep it safe or update if needed. 
        // The user's new Index.cshtml REMOVED the toggle button, so we can leave it or adapt it.
        // Let's keep it but ideally use Key for consistency if we updated JS, but user didn't include JS for toggle in their snippet.
        [HttpPost("ToggleActive/{id}")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var userId = _userManager.GetUserId(User);
            await _templateManager.ToggleActiveAsync(id, userId);
            return Ok(new { success = true });
        }
    }
}
