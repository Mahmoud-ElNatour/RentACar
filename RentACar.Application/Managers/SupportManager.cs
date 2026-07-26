using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs;
using RentACar.Application.DTOs.Support;
using RentACar.Core.Constants;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection; // For Scope
using Microsoft.AspNetCore.SignalR; // For HubContext
using RentACar.Application.Services;

namespace RentACar.Application.Managers
{
    public class SupportManager
    {
        private readonly ISupportConversationRepository _conversationRepository;
        private readonly ISupportMessageRepository _messageRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SupportManager> _logger;
        private readonly AuditLogManager _auditLogManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailManager _emailManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SupportManager(
            ISupportConversationRepository conversationRepository,
            ISupportMessageRepository messageRepository,
            ICustomerRepository customerRepository,
            IEmployeeRepository employeeRepository,
            IMapper mapper,
            ILogger<SupportManager> logger,
            AuditLogManager auditLogManager,
            UserManager<IdentityUser> userManager,
            EmailManager emailManager,
            AiManager aiManager,
            IServiceScopeFactory serviceScopeFactory)
        {
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
            _customerRepository = customerRepository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _logger = logger;
            _auditLogManager = auditLogManager;
            _userManager = userManager;
            _emailManager = emailManager;
            _aiManager = aiManager;
            _serviceScopeFactory = serviceScopeFactory;
        }

        private readonly AiManager _aiManager;

        // --- Customer Methods ---

        public async Task<PagedResultDto<SupportConversationListDto>> GetCustomerConversationsPagedAsync(string customerId, int page, int pageSize)
        {
            var query = _conversationRepository.Query()
                .Where(c => c.Customer.aspNetUserId == customerId)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(c => c.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.Messages)
                .ToListAsync();

            var dtos = items.Select(c => {
                var dto = _mapper.Map<SupportConversationListDto>(c);
                var lastMsg = c.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
                dto.LastMessageSnippet = lastMsg != null ? (lastMsg.MessageText.Length > 50 ? lastMsg.MessageText.Substring(0, 47) + "..." : lastMsg.MessageText) : "";
                dto.LastUpdatedAt = lastMsg?.CreatedAt ?? c.UpdatedAt;
                return dto;
            }).ToList();

            return new PagedResultDto<SupportConversationListDto>
            {
                Items = dtos,
                TotalCount = totalCount
            };
        }

        public async Task<SupportConversationDetailsDto?> GetConversationDetailsForCustomerAsync(int conversationId, string customerId)
        {
            var conversation = await _conversationRepository.Query()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Customer)
                    .ThenInclude(cu => cu.User)
                .Include(c => c.AssignedEmployee)
                .Include(c => c.Booking)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.SupportConversationId == conversationId && c.Customer.aspNetUserId == customerId);

            if (conversation == null) return null;

            // Filter out internal notes for customers
            var messages = conversation.Messages.Where(m => !m.IsInternalNote).OrderBy(m => m.CreatedAt).ToList();
            
            var dto = _mapper.Map<SupportConversationDetailsDto>(conversation);
            dto.Messages = _mapper.Map<List<SupportMessageDto>>(messages);
            dto.CustomerName = conversation.Customer?.Name ?? "Unknown";
            dto.CustomerEmail = conversation.Customer?.User?.Email;
            dto.CustomerPhone = conversation.Customer?.User?.PhoneNumber;
            dto.IsVerified = conversation.Customer?.IsVerified ?? false;
            dto.RealCustomerId = conversation.Customer?.UserId ?? 0;
            dto.AssignedEmployeeName = conversation.AssignedEmployee?.Name;

            return dto;
        }

        public async Task<int> CreateConversationAsync(string customerId, CreateSupportConversationDto dto)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null) throw new Exception("Customer record not found for user identity.");

            var conversation = new SupportConversation
            {
                CustomerId = customer.UserId,
                BookingId = dto.BookingId,
                Subject = dto.Subject,
                Category = dto.Category,
                Status = SupportStatus.Open,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _conversationRepository.AddAsync(conversation);
            await _conversationRepository.SaveChangesAsync();

            var initialMessage = new SupportMessage
            {
                SupportConversationId = conversation.SupportConversationId,
                SenderUserId = customerId,
                SenderRole = SenderRole.Customer,
                MessageText = dto.InitialMessage,
                CreatedAt = DateTime.UtcNow
            };

            await _messageRepository.AddAsync(initialMessage);
            await _messageRepository.SaveChangesAsync();

            await _auditLogManager.LogAsync("Create", "SupportConversation", conversation.SupportConversationId.ToString(), $"Customer {customerId} created support ticket: {dto.Subject}");

            return conversation.SupportConversationId;
        }

        public async Task<bool> SendMessageAsCustomerAsync(string customerId, SendSupportMessageDto dto)
        {
            var conversation = await _conversationRepository.Query()
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.SupportConversationId == dto.ConversationId);

            if (conversation == null || conversation.Customer.aspNetUserId != customerId) return false;

            if (conversation.Status == SupportStatus.Closed || conversation.Status == SupportStatus.Resolved) return false;

            var message = new SupportMessage
            {
                SupportConversationId = dto.ConversationId,
                SenderUserId = customerId,
                SenderRole = SenderRole.Customer,
                MessageText = dto.MessageText,
                AttachmentUrl = dto.AttachmentUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _messageRepository.AddAsync(message);
            
            // If it was Resolved, and customer replies, it might stay Resolved or go back to Assigned? 
            // User said: "open (unassigned), closed (customer side), resolved (by employee), assigned"
            // If customer replies to an Assigned ticket, it stays Assigned.
            // If customer replies to a Resolved ticket, should it remain Resolved or move back?
            // Usually it stays Assigned if someone is on it.
            
            
            conversation.UpdatedAt = DateTime.UtcNow;
            await _conversationRepository.UpdateAsync(conversation);

            await _messageRepository.SaveChangesAsync();
            await _conversationRepository.SaveChangesAsync();

            // --- AI Agent Trigger ---
            // Fire and forget so we don't block the HTTP request 
            // (In production, use BackgroundJob/Hangfire. Here valid to simple Task.Run for demo)
            _logger.LogInformation("AI Agent: Starting background task for conversation {ConversationId}", dto.ConversationId);
            
            _ = Task.Run(async () => 
            {
                try
                {
                    _logger.LogInformation("AI Agent: Background task started for conversation {ConversationId}", dto.ConversationId);
                    
                    using var scope = _serviceScopeFactory.CreateScope();
                    _logger.LogInformation("AI Agent: Service scope created");

                    // --- Update: Ensure AI User Exists (Fix for FK Constraint) ---
                    try 
                    {
                        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                        var aiUserId = "AI_AGENT";
                        var aiUser = await userManager.FindByIdAsync(aiUserId);
                        if (aiUser == null)
                        {
                            _logger.LogInformation("AI Agent: 'AI_AGENT' user missing. Creating it now...");
                            aiUser = new IdentityUser 
                            { 
                                Id = aiUserId, 
                                UserName = "ai_agent@rentacar.com", 
                                Email = "ai_agent@rentacar.com",
                                EmailConfirmed = true 
                            };
                            var result = await userManager.CreateAsync(aiUser);
                            if (!result.Succeeded)
                            {
                                _logger.LogError("AI Agent: Failed to create AI user. {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                            }
                            else
                            {
                                _logger.LogInformation("AI Agent: 'AI_AGENT' user created successfully.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "AI Agent: Error checking/creating AI user. Message saving might fail.");
                    }
                    // -----------------------------------------------------------
                    
                    var geminiService = scope.ServiceProvider.GetRequiredService<RentACar.Application.Services.IGeminiAgentService>();
                    _logger.LogInformation("AI Agent: GeminiService resolved");
                    
                    var contextManager = scope.ServiceProvider.GetRequiredService<AiSupportContextManager>();
                    _logger.LogInformation("AI Agent: ContextManager resolved");
                    
                    var messageRepo = scope.ServiceProvider.GetRequiredService<ISupportMessageRepository>();
                    var convRepo = scope.ServiceProvider.GetRequiredService<ISupportConversationRepository>();
                    _logger.LogInformation("AI Agent: Repositories resolved");

                    var freshConv = await convRepo.GetByIdAsync(dto.ConversationId);
                    if (freshConv == null)
                    {
                        _logger.LogWarning("AI Agent: Conversation {ConversationId} not found", dto.ConversationId);
                        return;
                    }
                    
                    if (freshConv.RequiresHumanIntervention)
                    {
                        _logger.LogInformation("AI Agent: Conversation {ConversationId} requires human intervention, skipping AI", dto.ConversationId);
                        return;
                    }

                    // Strategy: Only reply if unassigned OR no employee reply yet (excluding AI itself).
                    bool hasEmployeeReply = await messageRepo.Query().AnyAsync(m => m.SupportConversationId == dto.ConversationId && m.SenderRole == SenderRole.Employee && m.SenderUserId != "AI_AGENT");
                    _logger.LogInformation("AI Agent: HasEmployeeReply={HasEmployeeReply}", hasEmployeeReply);
                    
                    if (!hasEmployeeReply && !freshConv.RequiresHumanIntervention)
                    {
                        _logger.LogInformation("AI Agent: Fetching customer context for CustomerId={CustomerId}", freshConv.CustomerId);
                        var context = await contextManager.GetContextForCustomerAsync(freshConv.CustomerId);
                        
                        // Fetch history (last 6 messages)
                        var historyMessages = await messageRepo.Query()
                            .Where(m => m.SupportConversationId == dto.ConversationId)
                            .OrderByDescending(m => m.CreatedAt)
                            .Take(6)
                            .ToListAsync();
                        
                        var formattedHistory = historyMessages
                            .OrderBy(m => m.CreatedAt) // Oldest first
                            .Select(m => $"{(m.SenderRole == SenderRole.Employee ? (m.SenderUserId == "AI_AGENT" ? "AI" : "Agent") : "User")}: {m.MessageText}")
                            .ToList();

                        _logger.LogInformation("AI Agent: Generating AI response with {Count} history items...", formattedHistory.Count);
                        var response = await geminiService.GenerateResponseAsync(dto.MessageText, context, formattedHistory);

                        if (!string.IsNullOrEmpty(response))
                        {
                            _logger.LogInformation("AI Agent: Response generated, length={Length}", response.Length);
                            
                            // Save AI Response - SignalR Hub will broadcast when fetched
                            var aiMsg = new SupportMessage
                            {
                                SupportConversationId = dto.ConversationId,
                                SenderUserId = "AI_AGENT", // Special ID for AI
                                SenderRole = SenderRole.Employee, // Display as support agent
                                MessageText = response,
                                CreatedAt = DateTime.UtcNow
                            };
                            
                            
                            await messageRepo.AddAsync(aiMsg);
                            await messageRepo.SaveChangesAsync();
                            
                            _logger.LogInformation("AI Agent: Response saved successfully for conversation {ConversationId}", dto.ConversationId);

                            // --- SignalR Broadcast ---
                            try 
                            {
                                // Use the decoupled broadcaster interface
                                var broadcaster = scope.ServiceProvider.GetService<RentACar.Application.Services.ISignalRBroadcaster>();
                                
                                if (broadcaster != null)
                                {
                                    var msgDto = new SupportMessageDto
                                    {
                                        MessageId = aiMsg.SupportMessageId, // Fixed property name
                                        ConversationId = aiMsg.SupportConversationId, // Fixed property name
                                        SenderUserId = aiMsg.SenderUserId,
                                        SenderRole = aiMsg.SenderRole.ToString(),
                                        MessageText = aiMsg.MessageText,
                                        AttachmentUrl = aiMsg.AttachmentUrl,
                                        CreatedAt = aiMsg.CreatedAt,
                                        IsInternalNote = aiMsg.IsInternalNote
                                    };

                                    await broadcaster.BroadcastSupportMessageAsync(dto.ConversationId, msgDto);
                                    _logger.LogInformation("AI Agent: Response broadcasted via SignalR");
                                }
                                else
                                {
                                    _logger.LogWarning("AI Agent: ISignalRBroadcaster not registered, skipping broadcast");
                                }
                            }
                            catch (Exception hubEx)
                            {
                                 _logger.LogError(hubEx, "AI Agent: Failed to broadcast via SignalR");
                            }
                            // -------------------------
                        }
                        else
                        {
                            _logger.LogWarning("AI Agent: Empty response from Gemini API");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't crash
                    _logger.LogError(ex, "AI Agent: Error in background task processing for conversation {ConversationId}", dto.ConversationId);
                }
            });

            return true;
        }

        // --- Employee Methods ---

        public async Task<PagedResultDto<SupportConversationListDto>> GetAllConversationsPagedAsync(int page, int pageSize, string? status = null, string? category = null, string? searchQuery = null, string? assignedEmployeeId = null)
        {
            var query = _conversationRepository.Query()
                .Include(c => c.Customer).ThenInclude(c => c.User)
                .Include(c => c.AssignedEmployee)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(status)) query = query.Where(c => c.Status == status);
            if (!string.IsNullOrEmpty(category)) query = query.Where(c => c.Category == category);
            
            if (!string.IsNullOrEmpty(assignedEmployeeId))
            {
                query = query.Where(c => c.AssignedEmployee.aspNetUserId == assignedEmployeeId);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var lowerSearch = searchQuery.ToLower();
                query = query.Where(c => 
                    c.SupportConversationId.ToString().Contains(lowerSearch) ||
                    (c.Customer != null && (
                        c.Customer.Name.ToLower().Contains(lowerSearch) ||
                        (c.Customer.User != null && (
                            (c.Customer.User.Email != null && c.Customer.User.Email.ToLower().Contains(lowerSearch)) ||
                            (c.Customer.User.PhoneNumber != null && c.Customer.User.PhoneNumber.Contains(lowerSearch))
                        ))
                    ))
                );
            }

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(c => c.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.Messages)
                .ToListAsync();

            var dtos = items.Select(c => {
                var dto = _mapper.Map<SupportConversationListDto>(c);
                var lastMsg = c.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
                dto.LastMessageSnippet = lastMsg != null ? (lastMsg.MessageText.Length > 50 ? lastMsg.MessageText.Substring(0, 47) + "..." : lastMsg.MessageText) : "";
                dto.LastUpdatedAt = lastMsg?.CreatedAt ?? c.UpdatedAt;
                return dto;
            }).ToList();

            return new PagedResultDto<SupportConversationListDto>
            {
                Items = dtos,
                TotalCount = totalCount
            };
        }

        public async Task<SupportConversationDetailsDto?> GetConversationDetailsForEmployeeAsync(int conversationId, string employeeAspNetUserId)
        {
            var conversation = await _conversationRepository.Query()
                .AsSplitQuery()
                .Include(c => c.Customer)
                    .ThenInclude(cu => cu.User)
                .Include(c => c.AssignedEmployee)
                    .ThenInclude(e => e.User)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.Car)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.SupportConversationId == conversationId);

            if (conversation == null) return null;

            // --- Auto Assignment Logic ---
            if (conversation.Status == SupportStatus.Open && conversation.AssignedEmployeeId == null)
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeAspNetUserId);
                if (employee != null)
                {
                    conversation.AssignedEmployeeId = employee.EmployeeId;
                    conversation.Status = SupportStatus.Assigned;
                    conversation.UpdatedAt = DateTime.UtcNow;
                    await _conversationRepository.UpdateAsync(conversation);
                    await _conversationRepository.SaveChangesAsync();

                    // Automatic intro message
                    var introMsg = new SupportMessage
                    {
                        SupportConversationId = conversationId,
                        SenderUserId = employeeAspNetUserId,
                        SenderRole = SenderRole.Employee,
                        MessageText = $"Hi, I am {employee.Name} from LB Car Rental Customer support, and I will help you",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _messageRepository.AddAsync(introMsg);
                    await _messageRepository.SaveChangesAsync();

                    await _auditLogManager.LogAsync("Assign", "SupportConversation", conversationId.ToString(), $"Auto-assigned to {employee.Name} on first open.");
                    
                    // Reload conversation after update to get the assigned employee navigation property
                    return await GetConversationDetailsForEmployeeAsync(conversationId, employeeAspNetUserId);
                }
            }

            var sortedMessages = conversation.Messages.OrderBy(m => m.CreatedAt).ToList();

            var dto = _mapper.Map<SupportConversationDetailsDto>(conversation);
            dto.Messages = _mapper.Map<List<SupportMessageDto>>(sortedMessages);
            dto.CustomerName = conversation.Customer?.Name ?? "Unknown";
            dto.CustomerEmail = conversation.Customer?.User?.Email;
            dto.CustomerPhone = conversation.Customer?.User?.PhoneNumber;
            dto.IsVerified = conversation.Customer?.IsVerified ?? false;
            dto.RealCustomerId = conversation.Customer?.UserId ?? 0;
            dto.AssignedEmployeeName = conversation.AssignedEmployee?.Name;

            // Fetch active or last booking if not directly linked
            var booking = conversation.Booking;
            if (booking == null)
            {
                booking = await _conversationRepository.Query()
                    .Where(c => c.CustomerId == conversation.CustomerId && c.BookingId != null)
                    .OrderByDescending(c => c.CreatedAt)
                    .Include(c => c.Booking)
                        .ThenInclude(b => b.Car)
                    .Select(c => c.Booking)
                    .FirstOrDefaultAsync();
            }

            if (booking != null)
            {
                dto.ActiveBooking = new SupportBookingDto
                {
                    BookingId = booking.BookingId,
                    CarName = booking.Car?.ModelName ?? "Unknown Car",
                    PlateNumber = booking.Car?.PlateNumber ?? "N/A",
                    PickupDate = booking.PickupDateTime ?? conversation.CreatedAt,
                    ReturnDate = booking.Enddate.ToDateTime(TimeOnly.MinValue),
                    Status = booking.BookingStatus ?? "Unknown"
                };
            }

            // Fetch My Active Conversations (Assigned to Me and Not Closed/Resolved)
            var activeConversations = await _conversationRepository.Query()
                .Include(c => c.Customer)
                .Include(c => c.Messages)
                .Where(c => c.AssignedEmployee.aspNetUserId == employeeAspNetUserId && 
                            c.Status != SupportStatus.Closed && 
                            c.Status != SupportStatus.Resolved)
                .OrderByDescending(c => c.UpdatedAt)
                .Take(5)
                .ToListAsync();

            dto.MyActiveConversations = activeConversations.Select(c => {
                var listDto = _mapper.Map<SupportConversationListDto>(c);
                var lastMsg = c.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
                listDto.LastMessageSnippet = lastMsg != null ? (lastMsg.MessageText.Length > 30 ? lastMsg.MessageText.Substring(0, 27) + "..." : lastMsg.MessageText) : "No messages";
                listDto.LastUpdatedAt = lastMsg?.CreatedAt ?? c.UpdatedAt;
                return listDto;
            }).ToList();

            return dto;
        }

        public async Task<bool> SendMessageAsEmployeeAsync(string employeeAspNetUserId, SendSupportMessageDto dto)
        {
            var conversation = await _conversationRepository.Query()
                .Include(c => c.AssignedEmployee)
                .FirstOrDefaultAsync(c => c.SupportConversationId == dto.ConversationId);

            if (conversation == null) return false;

            // Permission check: Only Assigned Employee or Admin can reply
            bool isAdmin = await IsUserAdminAsync(employeeAspNetUserId);
            if (conversation.AssignedEmployee?.aspNetUserId != employeeAspNetUserId && !isAdmin)
            {
                return false;
            }

            if (conversation.Status == SupportStatus.Closed || conversation.Status == SupportStatus.Resolved) return false;

            var message = new SupportMessage
            {
                SupportConversationId = dto.ConversationId,
                SenderUserId = employeeAspNetUserId,
                SenderRole = SenderRole.Employee,
                MessageText = dto.MessageText,
                AttachmentUrl = dto.AttachmentUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _messageRepository.AddAsync(message);

            conversation.UpdatedAt = DateTime.UtcNow;
            await _conversationRepository.UpdateAsync(conversation);
            await _messageRepository.SaveChangesAsync();
            await _conversationRepository.SaveChangesAsync();

            return true;
        }

        private async Task<bool> IsUserAdminAsync(string aspNetUserId)
        {
            var user = await _userManager.FindByIdAsync(aspNetUserId);
            return user != null && await _userManager.IsInRoleAsync(user, "Admin");
        }

        public async Task AddInternalNoteAsync(string employeeId, int conversationId, string messageText)
        {
            var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            if (conversation == null) return;

            var message = new SupportMessage
            {
                SupportConversationId = conversationId,
                SenderUserId = employeeId,
                SenderRole = SenderRole.Employee,
                MessageText = messageText,
                IsInternalNote = true,
                CreatedAt = DateTime.UtcNow
            };

            await _messageRepository.AddAsync(message);
            await _messageRepository.SaveChangesAsync();
        }

        public async Task<bool> UpdateStatusAsync(string actorId, int conversationId, string newStatus)
        {
            var conversation = await _conversationRepository.Query()
                .FirstOrDefaultAsync(c => c.SupportConversationId == conversationId);
            
            if (conversation == null)
            {
                _logger.LogError($"Attempted to update status of non-existent conversation {conversationId}");
                return false;
            }

            string oldStatus = conversation.Status;
            conversation.Status = newStatus;
            conversation.UpdatedAt = DateTime.UtcNow;

            if (newStatus == SupportStatus.Closed)
            {
                conversation.ClosedAt = DateTime.UtcNow;
            }

            // Consolidate updates to ensure state is marked and saved once
            await _conversationRepository.UpdateAsync(conversation);
            await _conversationRepository.SaveChangesAsync();

            await _auditLogManager.LogAsync(
                action: "Update", 
                entity: "SupportConversation", 
                entityId: conversationId.ToString(), 
                summary: $"Actor {actorId} changed status from {oldStatus} to {newStatus}",
                status: "Success",
                oldValues: new { Status = oldStatus },
                newValues: new { Status = newStatus });
            _logger.LogInformation($"Support ticket {conversationId} status updated successfully to {newStatus} by {actorId}");
            return true;
        }

        public async Task ReassignAsync(string actorAspNetUserId, int conversationId, string targetEmployeeAspNetUserId, string note)
        {
            var conversation = await _conversationRepository.Query()
                .Include(c => c.AssignedEmployee)
                .FirstOrDefaultAsync(c => c.SupportConversationId == conversationId);
            if (conversation == null) return;

            var targetEmployee = await _employeeRepository.Query()
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.aspNetUserId == targetEmployeeAspNetUserId);
            if (targetEmployee == null) return;

            var oldEmployeeName = conversation.AssignedEmployee?.Name ?? "Unassigned";
            conversation.AssignedEmployeeId = targetEmployee.EmployeeId;
            conversation.Status = SupportStatus.Assigned;
            conversation.UpdatedAt = DateTime.UtcNow;

            await _conversationRepository.UpdateAsync(conversation);
            await _conversationRepository.SaveChangesAsync();

            if (!string.IsNullOrEmpty(note))
            {
                await AddInternalNoteAsync(actorAspNetUserId, conversationId, $"Reassignment Note: {note}");
            }

            await _auditLogManager.LogAsync("Reassign", "SupportConversation", conversationId.ToString(), $"Reassigned from {oldEmployeeName} to {targetEmployee.Name} by {actorAspNetUserId}");
            
            // Send Email Notification to new employee
            if (targetEmployee.User != null && !string.IsNullOrEmpty(targetEmployee.User.Email))
            {
                var details = $"A support ticket #{conversationId} has been reassigned to you.<br/><b>Note:</b> {note}";
                await _emailManager.SendInternalNotification(
                    new List<string> { targetEmployee.User.Email }, 
                    "New Support Ticket Assigned", 
                    "Ticket Reassignment",
                    details, 
                    "System");
            }
        }

        public async Task<SupportDashboardStatsDto> GetSupportStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            
            var openCount = await _conversationRepository.Query()
                .CountAsync(c => c.Status == SupportStatus.Open);

            var assignedCount = await _conversationRepository.Query()
                .CountAsync(c => c.Status == SupportStatus.Assigned);
                
            var resolvedTodayCount = await _conversationRepository.Query()
                .CountAsync(c => (c.Status == SupportStatus.Resolved || c.Status == SupportStatus.Closed) && c.UpdatedAt >= today);

            return new SupportDashboardStatsDto
            {
                OpenTicketsCount = openCount,
                WaitingForCustomerCount = assignedCount, // Reusing field for Assigned
                ResolvedTodayCount = resolvedTodayCount,
                OpenTrend = 0,
                WaitingTrend = 0,
                ResolvedTrend = 0
            };
        }

        public async Task<int> EscalateToTicketAsync(string userId, int aiConversationId)
        {
            // 1. Validate AI Conversation
            var aiMessages = await _aiManager.GetHistoryAsync(aiConversationId);
            if (!aiMessages.Any()) return 0;

            var customer = await _customerRepository.GetByIdAsync(userId);
            if (customer == null) throw new Exception("Customer not found");

            // 2. Create Support Conversation
            var conversation = new SupportConversation
            {
                CustomerId = customer.UserId,
                Subject = "Escalated AI Conversation",
                Category = "General",
                Status = SupportStatus.Open,
                RequiresHumanIntervention = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _conversationRepository.AddAsync(conversation);
            await _conversationRepository.SaveChangesAsync();

            // 3. Compile History
            var recentMessages = aiMessages.OrderByDescending(m => m.CreatedAt).Take(20).OrderBy(m => m.CreatedAt).ToList();
            var summaryText = "### Escalated Conversation History\n\n";
            
            foreach (var msg in recentMessages)
            {
                var role = msg.Sender == RentACar.Core.Enums.AiSenderType.User ? "Customer" : "AI";
                summaryText += $"**{role}**: {msg.Content}\n\n";
            }

            // 4. Create Initial Message
            var supportMsg = new SupportMessage
            {
                SupportConversationId = conversation.SupportConversationId,
                SenderUserId = userId,
                SenderRole = SenderRole.Customer,
                MessageText = "Please help me with the following inquiry (Escalated from AI Chat).",
                IsInternalNote = false,
                CreatedAt = DateTime.UtcNow
            };
            await _messageRepository.AddAsync(supportMsg);

            // 5. Add History as Internal Note
            var historyNote = new SupportMessage
            {
                SupportConversationId = conversation.SupportConversationId,
                SenderUserId = userId,
                SenderRole = SenderRole.Employee,
                MessageText = summaryText,
                IsInternalNote = true, 
                CreatedAt = DateTime.UtcNow.AddSeconds(1)
            };
            await _messageRepository.AddAsync(historyNote);
            await _messageRepository.SaveChangesAsync();

            // 6. Mark AI as Escalated
            await _aiManager.MarkEscalatedAsync(aiConversationId);

            await _auditLogManager.LogAsync("Escalate", "SupportConversation", conversation.SupportConversationId.ToString(), "Created from AI Chat #" + aiConversationId);

            return conversation.SupportConversationId;
        }
        public async Task<bool> EscalateToHumanAsync(int conversationId)
        {
            var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            if (conversation == null) return false;

            if (!conversation.RequiresHumanIntervention)
            {
                conversation.RequiresHumanIntervention = true;
                conversation.UpdatedAt = DateTime.UtcNow;
                await _conversationRepository.UpdateAsync(conversation);
                await _conversationRepository.SaveChangesAsync();

                await _auditLogManager.LogAsync("Escalate", "SupportConversation", conversationId.ToString(), "Ticket escalated to human agent by user request.");
                return true;
            }
            return true;
        }
    }

    public class SupportProfile : Profile
    {
        public SupportProfile()
        {
            CreateMap<SupportConversation, SupportConversationListDto>()
                .ForMember(dest => dest.ConversationId, opt => opt.MapFrom(src => src.SupportConversationId))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.Customer.User.Email))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Customer.User.PhoneNumber))
                .ForMember(dest => dest.AssignedEmployeeId, opt => opt.MapFrom(src => src.AssignedEmployee.aspNetUserId))
                .ForMember(dest => dest.AssignedEmployeeName, opt => opt.MapFrom(src => src.AssignedEmployee.Name));

            CreateMap<SupportConversation, SupportConversationDetailsDto>()
                .ForMember(dest => dest.ConversationId, opt => opt.MapFrom(src => src.SupportConversationId))
                .ForMember(dest => dest.RealCustomerId, opt => opt.MapFrom(src => src.Customer.UserId))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Customer.aspNetUserId))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.Customer.User.Email))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Customer.User.PhoneNumber))
                .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => src.Customer.IsVerified))
                .ForMember(dest => dest.AssignedEmployeeId, opt => opt.MapFrom(src => src.AssignedEmployee.aspNetUserId))
                .ForMember(dest => dest.Messages, opt => opt.Ignore())
                .ForMember(dest => dest.MyActiveConversations, opt => opt.Ignore())
                .ForMember(dest => dest.ActiveBooking, opt => opt.Ignore());

            CreateMap<SupportMessage, SupportMessageDto>()
                .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.SupportMessageId))
                .ForMember(dest => dest.SenderDisplayName, opt => opt.MapFrom(src => src.Sender.UserName));
        }
    }
}
