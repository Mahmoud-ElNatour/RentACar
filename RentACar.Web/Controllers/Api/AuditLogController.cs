using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs;
using RentACar.Application.Managers;
using System;
using System.Threading.Tasks;

namespace RentACar.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Employee")]
    public class AuditLogController : ControllerBase
    {
        private readonly AuditLogManager _auditLogManager;

        public AuditLogController(AuditLogManager auditLogManager)
        {
            _auditLogManager = auditLogManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs(string? search, string? actionType, string? entity, string? status, DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 10)
        {
            var (logs, totalCount) = await _auditLogManager.GetLogsAsync(search, actionType, entity, status, startDate, endDate, page, pageSize);

            var response = new
            {
                Logs = logs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(response);
        }
    }
}
