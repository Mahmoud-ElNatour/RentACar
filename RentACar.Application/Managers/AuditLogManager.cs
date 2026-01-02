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

        public async Task LogAsync(string action, string entity, string entityId, string summary, string status = "Success", string? explicitActorName = null, string? explicitActorRole = null)
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                string actorName = explicitActorName ?? user?.Identity?.Name ?? "System";
                string actorRole = explicitActorRole ?? "Unknown";
                // improved IP detection logic to handle proxies and load balancers
                string ipAddress = "Unknown";
                
                if (_httpContextAccessor.HttpContext?.Request?.Headers != null)
                {
                    var headers = _httpContextAccessor.HttpContext.Request.Headers;

                    // 1. Cloudflare Support
                    if (headers.ContainsKey("CF-Connecting-IP"))
                    {
                        ipAddress = headers["CF-Connecting-IP"].ToString();
                    }
                    // 2. Standard X-Forwarded-For (can be a comma-separated list)
                    else if (headers.ContainsKey("X-Forwarded-For"))
                    {
                        var forwardedFor = headers["X-Forwarded-For"].ToString();
                        if (!string.IsNullOrWhiteSpace(forwardedFor))
                        {
                            // The first IP in the list is the original client IP
                            ipAddress = forwardedFor.Split(',')[0].Trim();
                        }
                    }
                    // 3. Nginx / Standard Proxy "Real IP" header
                    else if (headers.ContainsKey("X-Real-IP"))
                    {
                        ipAddress = headers["X-Real-IP"].ToString();
                    }
                }
                
                // 4. Fallback to the direct connection IP if headers didn't yield a result
                if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "Unknown")
                {
                     // Ensure RemoteIpAddress is not null before checking AddressFamily or converting
                     var remoteIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress;
                     if (remoteIp != null)
                     {
                         // If it's an IPv6-mapped IPv4, map it back to IPv4 for readability
                         if (remoteIp.IsIPv4MappedToIPv6)
                         {
                             remoteIp = remoteIp.MapToIPv4();
                         }
                         ipAddress = remoteIp.ToString();
                     }
                }
                string userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";

                if (user != null && explicitActorRole == null)
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

        public async Task<(List<AuditLog> Logs, int TotalCount)> GetLogsAsync(string? searchTerm, string? actionType, string? entityName, string? status, DateTime? startDate, DateTime? endDate, int page, int pageSize)
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

            if (startDate.HasValue)
            {
                query = query.Where(l => l.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                // Include the entire end day
                var endProp = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(l => l.Timestamp <= endProp);
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
