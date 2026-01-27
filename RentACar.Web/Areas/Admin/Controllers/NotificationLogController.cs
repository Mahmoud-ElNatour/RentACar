using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentACar.Infrastructure.Data;
using RentACar.Web.Areas.Admin.ViewModels.EmailServices;

namespace RentACar.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class NotificationLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string eventType,
            string result,
            string search,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1)
        {
            int pageSize = 20;
            var query = _context.NotificationLogs.AsQueryable();

            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(l => l.EventType == eventType);

            if (!string.IsNullOrEmpty(result))
                query = query.Where(l => l.Result == result);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(l => 
                    l.TargetId.ToLower().Contains(search) || 
                    l.RecipientEmail.ToLower().Contains(search));
            }

            if (fromDate.HasValue)
                query = query.Where(l => l.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.CreatedAt <= toDate.Value);

            var totalCount = await query.CountAsync();
            var logs = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new NotificationLogListVM
            {
                Logs = logs,
                EventType = eventType,
                Result = result,
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
        public async Task<IActionResult> Details(int id)
        {
             var log = await _context.NotificationLogs.FindAsync(id);
             if (log == null) return NotFound();
             return PartialView("_NotificationLogDetailsModal", log);
        }

        [HttpGet]
        public async Task<IActionResult> Export(
            string eventType,
            string result,
            string search,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.NotificationLogs.AsQueryable();

            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(l => l.EventType == eventType);

            if (!string.IsNullOrEmpty(result))
                query = query.Where(l => l.Result == result);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(l => 
                    l.TargetId.ToLower().Contains(search) || 
                    l.RecipientEmail.ToLower().Contains(search));
            }

            if (fromDate.HasValue)
                query = query.Where(l => l.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.CreatedAt <= toDate.Value);

            var logs = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Time,Event,Target,Recipient,Result,Details");

            foreach (var log in logs)
            {
                // Simple CSV escaping
                var details = log.Details?.Replace(",", " ")?.Replace("\n", " ") ?? "";
                builder.AppendLine($"{log.CreatedAt:yyyy-MM-dd HH:mm:ss},{log.EventType},{log.TargetId},{log.RecipientEmail},{log.Result},{details}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"notification_logs_{DateTime.Now:yyyyMMddHHmm}.csv");
        }
    }
}
