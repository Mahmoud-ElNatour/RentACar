using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.Managers;
using RentACar.Core.Entities;
using RentACar.Web.ViewModels;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/EmailServices/NotificationsEmailManagement")]
    public class EmailConfigurationCenterController : Controller
    {
        private readonly NotificationProcessingService _processingService;
        private readonly EmailProviderSettingsManager _providerManager;
        private readonly SenderIdentityManager _senderManager;
        private readonly EmailFeatureConfigManager _featureManager;
        private readonly EmailTemplateManager _templateManager;
        private readonly DistributionListManager _distListManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailManager _emailManager;

        public EmailConfigurationCenterController(
            NotificationProcessingService processingService,
            EmailProviderSettingsManager providerManager,
            SenderIdentityManager senderManager,
            EmailFeatureConfigManager featureManager,
            EmailTemplateManager templateManager,
            DistributionListManager distListManager,
            UserManager<IdentityUser> userManager,
            EmailManager emailManager)
        {
            _processingService = processingService;
            _providerManager = providerManager;
            _senderManager = senderManager;
            _featureManager = featureManager;
            _templateManager = templateManager;
            _distListManager = distListManager;
            _userManager = userManager;
            _emailManager = emailManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var vm = new EmailConfigurationCenterVM
            {
                NotificationSettings = await _processingService.GetSettingsAsync(),
                ProviderSettings = await _providerManager.GetSettingsAsync(),
                SenderIdentities = await _senderManager.GetAllIdentitiesAsync(),
                FeatureConfigs = await _featureManager.GetAllConfigsAsync(),
                AvailableTemplates = await _templateManager.GetAllTemplatesAsync()
            };
            
            // Need to implement GetAllListsAsync public method in DistManager if not present, assume yes for now
            // Actually DistListManager usually returns DTOs or Entities. Let's check or assume generic GetAll
            vm.AvailableDistributionLists = await _distListManager.GetAllListsAsync();

            // Fetch Last Run Record
            vm.LastRunRecord = await _processingService.GetLastRunRecordAsync();

            return View("~/Views/EmailServices/EmailConfigurationCenter/Index.cshtml", vm);
        }

        [HttpPost("TestProvider")]
        public async Task<IActionResult> TestProvider(string toEmail, string subject, string body)
        {
            try
            {
                await _emailManager.SendTestEmailAsync(toEmail, subject, body);
                TempData["Success"] = "Test email sent successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Test failed: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost("Save")]
        public async Task<IActionResult> Save(EmailConfigurationCenterVM model)
        {
            var userId = _userManager.GetUserId(User);

            // 1. Save Notification Settings
            await _processingService.UpdateSettingsAsync(model.NotificationSettings, userId);

            // 2. Save Provider Settings
            await _providerManager.UpdateSettingsAsync(model.ProviderSettings, userId);

            // 3. Save Feature Configs (Iterate and update)
             if (model.FeatureConfigs != null)
             {
                 foreach (var config in model.FeatureConfigs)
                 {
                     await _featureManager.UpdateConfigAsync(config, userId);
                 }
             }

            return RedirectToAction("Index");
        }

        // --- SENDER IDENTITY CRUD ---
        [HttpPost("Sender/Create")]
        public async Task<IActionResult> CreateSender(SenderIdentity identity)
        {
            var userId = _userManager.GetUserId(User);
            await _senderManager.CreateIdentityAsync(identity, userId);
            return RedirectToAction("Index");
        }

        [HttpPost("Sender/Edit")]
        public async Task<IActionResult> EditSender(SenderIdentity identity)
        {
            var userId = _userManager.GetUserId(User);
            await _senderManager.UpdateIdentityAsync(identity, userId);
            return RedirectToAction("Index");
        }

        [HttpPost("Sender/Delete/{id}")]
        public async Task<IActionResult> DeleteSender(int id)
        {
            await _senderManager.DeleteIdentityAsync(id);
            return RedirectToAction("Index");
        }
        
        [HttpPost("Sender/ToggleActive/{id}")]
        public async Task<IActionResult> ToggleSenderActive(int id)
        {
            var userId = _userManager.GetUserId(User);
            await _senderManager.ToggleActiveAsync(id, userId);
            // Return JSON/Ok if AJAX, or Redirect if simple
            return RedirectToAction("Index");
        }

        [HttpPost("RunNow")]
        public async Task<IActionResult> RunNow()
        {
             var userId = _userManager.GetUserId(User);
             await _processingService.RunOnceAsync("Admin:" + userId);
             return RedirectToAction("Index");
        }
    }
}
