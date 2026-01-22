using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.Managers;
using RentACar.Core.Entities;
using System.Threading.Tasks;
using System;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/EmailServices/Templates")]
    public class TemplatesController : Controller
    {
        private readonly EmailTemplateManager _emailTemplateManager;
        private readonly UserManager<IdentityUser> _userManager;

        public TemplatesController(EmailTemplateManager emailTemplateManager, UserManager<IdentityUser> userManager)
        {
            _emailTemplateManager = emailTemplateManager;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var templates = await _emailTemplateManager.GetAllTemplatesAsync();
            return View("~/Views/EmailServices/Templates/Index.cshtml", templates);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/EmailServices/Templates/Create.cshtml", new EmailTemplate { IsActive = true });
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmailTemplate model)
        {
            // Simple validation
            if (string.IsNullOrWhiteSpace(model.TemplateKey) || string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("", "Key and Name are required");
                return View("~/Views/EmailServices/Templates/Create.cshtml", model);
            }

            var existing = await _emailTemplateManager.GetTemplateByKeyAsync(model.TemplateKey);
            if (existing != null)
            {
                ModelState.AddModelError("TemplateKey", "This key already exists.");
                return View("~/Views/EmailServices/Templates/Create.cshtml", model);
            }

            model.UpdatedAt = DateTime.UtcNow;
            model.UpdatedByUserId = _userManager.GetUserId(User) ?? string.Empty;

            await _emailTemplateManager.CreateTemplateAsync(model);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{key}")]
        public async Task<IActionResult> Edit(string key)
        {
            var template = await _emailTemplateManager.GetTemplateByKeyAsync(key);
            if (template == null) return NotFound();
            return View("~/Views/EmailServices/Templates/Edit.cshtml", template);
        }

        [HttpPost("Edit/{key}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string key, EmailTemplate model)
        {
            if (key != model.TemplateKey) return BadRequest();

            var template = await _emailTemplateManager.GetTemplateByKeyAsync(key);
            if (template == null) return NotFound();

            template.Subject = model.Subject;
            template.Body = model.Body;
            template.Category = model.Category;
            // Name and Key might be read-only ideally, but respecting the form
            template.Name = model.Name;
            template.IsActive = model.IsActive;
            template.UpdatedAt = DateTime.UtcNow;
            template.UpdatedByUserId = _userManager.GetUserId(User) ?? string.Empty;

            await _emailTemplateManager.UpdateTemplateAsync(template);

            // Redirect to list or stay on page with success message
            // For now redirect to Index
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Reset/{key}")]
        public async Task<IActionResult> Reset(string key)
        {
            await _emailTemplateManager.ResetTemplateToDefaultAsync(key);
            return RedirectToAction(nameof(Edit), new { key });
        }

        [HttpPost("Delete/{key}")]
        public async Task<IActionResult> Delete(string key)
        {
            await _emailTemplateManager.DeleteTemplateAsync(key);
            return RedirectToAction(nameof(Index));
        }
    }
}
