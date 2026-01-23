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
            return View("~/Views/EmailServices/Templates/Create.cshtml");
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(EmailTemplate template)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/EmailServices/Templates/Create.cshtml", template);
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                 // Normalize key if needed
                template.TemplateKey = template.TemplateKey.Trim().Replace(" ", "_");
                
                await _templateManager.CreateTemplateAsync(template, userId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/EmailServices/Templates/Create.cshtml", template);
            }
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var template = await _templateManager.GetTemplateByIdAsync(id);
            if (template == null) return NotFound();

            return View("~/Views/EmailServices/Templates/Edit.cshtml", template);
        }

        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, EmailTemplate template)
        {
             if (id != template.Id) return BadRequest();
             
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

        [HttpGet("Preview/{id}")]
        public async Task<IActionResult> Preview(int id)
        {
             var template = await _templateManager.GetTemplateByIdAsync(id);
             if (template == null) return NotFound();
             
             // In a real scenario, we might inject sample data here for placeholders
             // But for now, just show the body
             return View("~/Views/EmailServices/Templates/Preview.cshtml", template);
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _templateManager.DeleteTemplateAsync(id);
            return Ok(new { success = true });
        }

        [HttpPost("ToggleActive/{id}")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var userId = _userManager.GetUserId(User);
            await _templateManager.ToggleActiveAsync(id, userId);
            return Ok(new { success = true });
        }
    }
}
