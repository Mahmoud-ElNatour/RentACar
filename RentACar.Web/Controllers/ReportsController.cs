using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly AuditLogManager _auditLogManager;

        public ReportsController(AuditLogManager auditLogManager)
        {
            _auditLogManager = auditLogManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AuditLog(string? search, string? actionType, string? entity, string? status, DateTime? startDate, DateTime? endDate)
        {
            var (actions, entities) = await _auditLogManager.GetDistinctFiltersAsync();

            var model = new AuditLogViewModel
            {
                SearchTerm = search,
                ActionType = actionType,
                EntityName = entity,
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                AvailableActions = actions,
                AvailableEntities = entities
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportAuditLog(string? search, string? actionType, string? entity, string? status, DateTime? startDate, DateTime? endDate)
        {
            // Get all logs matching the criteria (no pagination)
            var (logs, _) = await _auditLogManager.GetLogsAsync(search, actionType, entity, status, startDate, endDate, 1, int.MaxValue);

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Timestamp,Actor,Role,Action,Entity,EntityId,Summary,IP Address,Status");

            foreach (var log in logs)
            {
                builder.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},{EscapeCsv(log.ActorName)},{EscapeCsv(log.ActorRole)},{EscapeCsv(log.Action)},{EscapeCsv(log.Entity)},{EscapeCsv(log.EntityId)},{EscapeCsv(log.Summary)},{log.IpAddress},{log.Status}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"AuditLogs_{System.DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        private string EscapeCsv(string? field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}