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

        public async Task LogAsync(string action, string entity, string entityId, string summary, string status = "Success", string? explicitActorName = null, string? explicitActorRole = null, object? oldValues = null, object? newValues = null)
        {
            await LogEventAsync(action, entity, entityId, summary, null, status, null, explicitActorName, explicitActorRole, null, status, null, null, oldValues, newValues);
        }

        public async Task LogEventAsync(
            string action,
            string entity,
            string entityId,
            string summary,
            Dictionary<string, object>? details = null,
            string status = "Success",
            string? failureReason = null,
            string? explicitActorName = null,
            string? explicitActorRole = null,
            string? correlationId = null,
            string? outcome = null,
            string? targetType = null,
            string? targetId = null,
            object? oldValues = null,
            object? newValues = null)
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                string actorName = explicitActorName ?? user?.Identity?.Name ?? "System";
                string actorRole = explicitActorRole ?? "Unknown";
                
                string ipAddress = "Unknown";
                
                if (_httpContextAccessor.HttpContext?.Request?.Headers != null)
                {
                    var headers = _httpContextAccessor.HttpContext.Request.Headers;

                    if (headers.ContainsKey("CF-Connecting-IP"))
                    {
                        ipAddress = headers["CF-Connecting-IP"].ToString();
                    }
                    else if (headers.ContainsKey("X-Forwarded-For"))
                    {
                        var forwardedFor = headers["X-Forwarded-For"].ToString();
                        if (!string.IsNullOrWhiteSpace(forwardedFor))
                        {
                            ipAddress = forwardedFor.Split(',')[0].Trim();
                        }
                    }
                    else if (headers.ContainsKey("X-Real-IP"))
                    {
                        ipAddress = headers["X-Real-IP"].ToString();
                    }
                }
                
                if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "Unknown")
                {
                     var remoteIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress;
                     if (remoteIp != null)
                     {
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
                    var roles = user.FindAll(ClaimTypes.Role);
                    if (roles.Any())
                    {
                        actorRole = string.Join(", ", roles.Select(r => r.Value));
                    }
                }

                string? userId = null;
                if (user != null && user.Identity?.IsAuthenticated == true)
                {
                     userId = _userManager.GetUserId(user);
                }

                var log = new AuditLog
                {
                    Timestamp = DateTime.UtcNow,
                    UserId = userId,
                    ActorName = actorName,
                    ActorRole = actorRole,
                    Action = action,
                    Entity = entity,
                    EntityId = entityId,
                    Summary = summary,
                    IpAddress = ipAddress,
                    Device = ParseDevice(userAgent),
                    Status = status,
                    FailureReason = failureReason,
                    CorrelationId = correlationId,
                    UserAgent = userAgent,
                    Outcome = outcome ?? status, // Default Outcome to Status if not provided
                    TargetType = targetType ?? entity, // Default TargetType to Entity
                    TargetId = targetId ?? entityId // Default TargetId to EntityId
                };

                if (details != null && details.Count > 0)
                {
                    log.DetailsJson = System.Text.Json.JsonSerializer.Serialize(details);
                }

                if (oldValues != null)
                {
                    log.OldValuesJson = System.Text.Json.JsonSerializer.Serialize(oldValues);
                }

                if (newValues != null)
                {
                    log.NewValuesJson = System.Text.Json.JsonSerializer.Serialize(newValues);
                }

                _dbContext.AuditLogs.Add(log);
                await _dbContext.SaveChangesAsync();
            }
            catch
            {
                // Fail silently
            }
        }

        public async Task<(List<AuditLogDto> Logs, int TotalCount)> GetLogsAsync(string? searchTerm, string? actionType, string? entityName, string? status, DateTime? startDate, DateTime? endDate, int page, int pageSize)
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
                if (actionType.Equals("Email", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(l => l.Action == "EmailSent" || l.Action == "EmailFailed");
                }
                else if (actionType.Equals("Create", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(l => l.Action == "Create" || l.Action.Contains("Created") || l.Action.Contains("Registered"));
                }
                else if (actionType.Equals("Update", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(l => l.Action == "Update" || l.Action.Contains("Updated") || l.Action.Contains("Modified") || l.Action.Contains("StatusChanged"));
                }
                else if (actionType.Equals("Delete", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(l => l.Action == "Delete" || l.Action.Contains("Deleted") || l.Action.Contains("Cancelled"));
                }
                else if (actionType.Equals("Login", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(l => l.Action == "Login");
                }
                else if (actionType.Equals("Other", StringComparison.OrdinalIgnoreCase))
                {
                    // Exclude the ones covered by the categories above
                    var primaryActions = new[] { "Create", "Update", "Delete", "EmailSent", "EmailFailed", "Login" };
                    query = query.Where(l => !primaryActions.Contains(l.Action) && 
                                           !l.Action.Contains("Created") && 
                                           !l.Action.Contains("Updated") && 
                                           !l.Action.Contains("Modified") && 
                                           !l.Action.Contains("Deleted") &&
                                           !l.Action.Contains("Registered"));
                }
                else
                {
                    query = query.Where(l => l.Action == actionType);
                }
            }

            if (!string.IsNullOrWhiteSpace(entityName))
            {
                if (entityName.Equals("Other", StringComparison.OrdinalIgnoreCase))
                {
                    // Define "known" entities that should NOT be in "Other"
                    var knownEntities = new[] { "Car", "Category", "Customer", "Employee", "Booking", "Payment", "Promocode", "User", "ApplicationUser", "BlackList", "CreditCard" };
                    query = query.Where(l => !knownEntities.Contains(l.Entity));
                }
                else
                {
                    query = query.Where(l => l.Entity == entityName);
                }
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
                                  .Select(l => new AuditLogDto
                                  {
                                      Id = l.Id,
                                      Timestamp = l.Timestamp,
                                      ActorName = l.ActorName,
                                      ActorRole = l.ActorRole,
                                      Action = l.Action,
                                      Entity = l.Entity,
                                      EntityId = l.EntityId,
                                      Summary = l.Summary,
                                      IpAddress = l.IpAddress,
                                      Device = l.Device,
                                      Status = l.Status,
                                      TargetType = l.TargetType,
                                      TargetId = l.TargetId,
                                      Outcome = l.Outcome,
                                      DetailsJson = l.DetailsJson,
                                      OldValuesJson = l.OldValuesJson,
                                      NewValuesJson = l.NewValuesJson,
                                      FailureReason = l.FailureReason
                                  })
                                  .ToListAsync();

            return (logs, totalCount);
        }

        public async Task<(List<string> Actions, List<string> Entities)> GetDistinctFiltersAsync()
        {
            // The user wants specific categorized actions
            var actions = new List<string> { "Create", "Update", "Delete", "Email", "Login", "Other" };

            // Fetch primary entities for the "main" list
            var knownEntities = new List<string> { "Car", "Category", "Customer", "Employee", "Booking", "Payment", "Promocode", "User", "BlackList", "CreditCard" };
            
            var entities = await _dbContext.AuditLogs
                .Select(l => l.Entity)
                .Distinct()
                .Where(e => knownEntities.Contains(e))
                .OrderBy(e => e)
                .ToListAsync();

            // Append "Other" to the entities list
            entities.Add("Other");

            return (actions, entities);
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
