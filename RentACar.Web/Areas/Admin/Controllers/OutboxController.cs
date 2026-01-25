using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.Managers;
using RentACar.Web.Areas.Admin.ViewModels.EmailServices;
using System.Security.Claims;

namespace RentACar.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class OutboxController : Controller
    {
        private readonly EmailLogManager _emailLogManager;

        public OutboxController(EmailLogManager emailLogManager)
        {
            _emailLogManager = emailLogManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string status, 
            string type, 
            string search, 
            DateTime? fromDate, 
            DateTime? toDate, 
            int page = 1)
        {
            int pageSize = 20;

            var logs = await _emailLogManager.GetLogsAsync(status, type, search, fromDate, toDate, page, pageSize);
            var totalCount = await _emailLogManager.GetLogsCountAsync(status, type, search, fromDate, toDate);

            var vm = new EmailLogListVM
            {
                Logs = logs,
                Status = status,
                EmailType = type,
                SearchTerm = search,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Export(
            string status, 
            string type, 
            string search, 
            DateTime? fromDate, 
            DateTime? toDate)
        {
            // Use Manager with MaxInt page size to get all
            var logs = await _emailLogManager.GetLogsAsync(status, type, search, fromDate, toDate, 1, int.MaxValue);

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Time,Status,Type,Recipient,Subject,Attempts,Error");

            foreach (var log in logs)
            {
                var subject = log.Subject?.Replace(",", " ") ?? "";
                var recipient = log.RecipientsRaw?.Replace(",", ";") ?? ""; 
                var error = log.LastError?.Replace(",", " ")?.Replace("\n", " ") ?? "";
                
                builder.AppendLine($"{log.CreatedAt:yyyy-MM-dd HH:mm:ss},{log.Status},{log.EmailType},{recipient},{subject},{log.Attempts},{error}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"email_outbox_{DateTime.Now:yyyyMMddHHmm}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var log = await _emailLogManager.GetLogByIdAsync(id);
            if (log == null) return NotFound();
            return PartialView("_EmailLogDetailsModal", log); // Or JSON or View
        }



        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retry(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
           
            if (await _emailLogManager.RetryFailedEmailAsync(id, userId))
            {
                TempData["Success"] = "Email re-sent successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to resend email. Check details.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetryAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await _emailLogManager.RetryAllFailedAsync(userId);
            
            TempData["Success"] = $"Retried {count} emails.";
            return RedirectToAction(nameof(Index));
        }
    }
}
