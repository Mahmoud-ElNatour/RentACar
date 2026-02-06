using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RentACar.Application.Managers;
using RentACar.Application.Services;
using RentACar.Core.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RentACar.Core.Entities;

namespace RentACar.Web.Hubs
{
    [Authorize]
    public class AiChatHub : Hub
    {
        private readonly AiManager _aiManager;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly SupportManager _supportManager;

        public AiChatHub(AiManager aiManager, IServiceScopeFactory scopeFactory, SupportManager supportManager)
        {
            _aiManager = aiManager;
            _scopeFactory = scopeFactory;
            _supportManager = supportManager;
        }

        public async Task JoinWidget()
        {
            var userIdStr = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userIdStr)) return;

            // 1. Get Customer ID (Assuming int map or look up)
            // We need a way to map AspNetUserId -> CustomerId. 
            // AiManager expects int customerId.
            // Using SupportManager or Repo to lookup?
            // Let's assume we can resolve it. Ideally AiManager could handle string userId lookup?
            // For now, I'll allow AiManager to accept string or do lookup here.
            // I'll grab it via scope since I don't have CustomerRepo injected directly (to keep Hub clean? No, inject what I need).
            
            using (var scope = _scopeFactory.CreateScope())
            {
                 var customerRepo = scope.ServiceProvider.GetRequiredService<RentACar.Core.Repositories.ICustomerRepository>();
                 var customer = await customerRepo.GetByIdAsync(userIdStr);
                 if (customer == null) 
                 {
                     await Clients.Caller.SendAsync("Error", "Customer profile not found.");
                     return;
                 }

                 var conversation = await _aiManager.GetOrCreateConversationAsync(customer.UserId);
                 
                 // Send history
                 var history = await _aiManager.GetHistoryAsync(conversation.AiConversationId);
                 var historyDtos = history.Select(m => new 
                 {
                     sender = m.Sender.ToString(),
                     content = m.Content,
                     createdAt = m.CreatedAt
                 });

                 await Clients.Caller.SendAsync("HistoryLoaded", historyDtos);
                 await Clients.Caller.SendAsync("ConversationId", conversation.AiConversationId);
            }
        }

        public async Task SendMessage(int conversationId, string messageText)
        {
             var userIdStr = Context.UserIdentifier;
             if (string.IsNullOrEmpty(userIdStr)) return;

             await _aiManager.SaveMessageAsync(conversationId, messageText, AiSenderType.User);
             
             // Echo back
             await Clients.Caller.SendAsync("ReceiveMessage", "User", messageText);

             // Trigger AI
             _ = Task.Run(async () => await ProcessAiResponse(conversationId, userIdStr, messageText));
        }

        public async Task Escalate()
        {
             var userIdStr = Context.UserIdentifier;
             // We need conversationID.
             // We can find the active one for this user.
             using (var scope = _scopeFactory.CreateScope())
             {
                 var customerRepo = scope.ServiceProvider.GetRequiredService<RentACar.Core.Repositories.ICustomerRepository>();
                 var customer = await customerRepo.GetByIdAsync(userIdStr);
                 if(customer == null) return;

                 var aiManager = scope.ServiceProvider.GetRequiredService<AiManager>();
                 var conversation = await aiManager.GetOrCreateConversationAsync(customer.UserId);
                 
                 var supportManager = scope.ServiceProvider.GetRequiredService<SupportManager>();
                 var ticketId = await supportManager.EscalateToTicketAsync(userIdStr, conversation.AiConversationId);
                 
                 await Clients.Caller.SendAsync("Escalated", ticketId);
             }
        }

        private async Task ProcessAiResponse(int conversationId, string userId, string userMessage)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var geminiService = scope.ServiceProvider.GetRequiredService<IGeminiAgentService>();
                    var contextManager = scope.ServiceProvider.GetRequiredService<AiSupportContextManager>();
                    var aiManager = scope.ServiceProvider.GetRequiredService<AiManager>();
                    var customerRepo = scope.ServiceProvider.GetRequiredService<RentACar.Core.Repositories.ICustomerRepository>();

                    var customer = await customerRepo.GetByIdAsync(userId);
                    var context = await contextManager.GetContextForCustomerAsync(customer.UserId); // Need to optimize this later
                    
                    var history = await aiManager.GetHistoryAsync(conversationId);
                    
                    // Format history for Gemini (last 6?)
                    var formattedHistory = history.OrderByDescending(m => m.CreatedAt).Take(6).OrderBy(m => m.CreatedAt)
                        .Select(m => $"{m.Sender}: {m.Content}")
                        .ToList();

                    var response = await geminiService.GenerateResponseAsync(userMessage, context, formattedHistory);

                    if (!string.IsNullOrEmpty(response))
                    {
                        await aiManager.SaveMessageAsync(conversationId, response, AiSenderType.AI);
                        
                        // Send to User
                        // Since I don't have the "Clients" context here easily without IHubContext, 
                        // I need to inject IHubContext<AiChatHub> into this scope? 
                        // Actually, Hub methods are transient. I can't call Clients.Caller from background thread.
                        // I must use IHubContext.
                        
                        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<AiChatHub>>();
                        // How to target specific user? UserIdentifier should work map to User.
                        await hubContext.Clients.User(userId).SendAsync("ReceiveMessage", "AI", response);
                    }
                }
            }
            catch (Exception ex)
            {
                // check logger
            }
        }
    }
}
