using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using RentACar.Core.Entities;
using RentACar.Infrastructure.Data;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using RentACar.Application.DTOs;

namespace RentACar.Application.Managers
{
    public class AuditLogManager
    {
        private readonly RentACarDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public AuditLogManager(RentACarDbContext dbContext, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task LogAsync(string action, string entity, string entityId, string summary, string status = "Success")
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                string actorName = user?.Identity?.Name ?? "System";
                string actorRole = "Unknown";
                string ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
                string userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";

                if (user != null)
                {
                    // Basic role check - takes the first role found
                    var roles = user.FindAll(ClaimTypes.Role);
                    if (roles.Any())
                    {
                        actorRole = string.Join(", ", roles.Select(r => r.Value));
                    }
                }

                var log = new AuditLog
                {
                    Timestamp = DateTime.UtcNow,
                    ActorName = actorName,
                    ActorRole = actorRole,
                    Action = action,
                    Entity = entity,
                    EntityId = entityId,
                    Summary = summary,
                    IpAddress = ipAddress,
                    Device = ParseDevice(userAgent),
                    Status = status
                };

                _dbContext.AuditLogs.Add(log);
                await _dbContext.SaveChangesAsync();
            }
            catch
            {
                // Fail silently to not disrupt the main flow
                // In a real production app, we might log this to a file logger
            }
        }

        public async Task<(List<AuditLog> Logs, int TotalCount)> GetLogsAsync(string? searchTerm, string? actionType, string? entityName, string? status, int page, int pageSize)
        {
            var query = _dbContext.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(l => 
                    l.ActorName.ToLower().Contains(searchTerm) || 
                    l.Summary.ToLower().Contains(searchTerm) || 
                    l.EntityId.Contains(searchTerm) ||
                    l.IpAddress.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(actionType))
            {
                query = query.Where(l => l.Action == actionType);
            }

            if (!string.IsNullOrWhiteSpace(entityName))
            {
                query = query.Where(l => l.Entity == entityName);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(l => l.Status == status);
            }

            var totalCount = await query.CountAsync();
            var logs = await query.OrderByDescending(l => l.Timestamp)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            return (logs, totalCount);
        }

        private string ParseDevice(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";
            
            // Very basic parsing
            if (userAgent.Contains("Chrome")) return "Chrome";
            if (userAgent.Contains("Firefox")) return "Firefox";
            if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) return "Safari";
            if (userAgent.Contains("Edge")) return "Edge";
            
            if (userAgent.Length > 50) return userAgent.Substring(0, 47) + "...";
            return userAgent;
        }
    }
}
