using Microsoft.AspNetCore.SignalR;
using RentACar.Application.Managers;
using RentACar.Application.DTOs.Support;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace RentACar.Web.Hubs
{
    [Authorize]
    public class SupportChatHub : Hub
    {
        private readonly SupportManager _supportManager;

        public SupportChatHub(SupportManager supportManager)
        {
            _supportManager = supportManager;
        }

        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"support_conversation_{conversationId}");
            await Clients.Caller.SendAsync("Joined", conversationId);
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"support_conversation_{conversationId}");
        }

        public async Task SendMessage(int conversationId, string messageText, string? attachmentUrl = null)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) return;

            bool isAdmin = Context.User.IsInRole("Admin");
            bool isEmployee = Context.User.IsInRole("Employee") || isAdmin;
            
            if (string.IsNullOrWhiteSpace(messageText)) return;

            bool success = false;
            SendSupportMessageDto dto = new SendSupportMessageDto
            {
                ConversationId = conversationId,
                MessageText = messageText,
                AttachmentUrl = attachmentUrl
            };

            if (isEmployee)
            {
                success = await _supportManager.SendMessageAsEmployeeAsync(userId, dto);
            }
            else
            {
                success = await _supportManager.SendMessageAsCustomerAsync(userId, dto);
            }

            if (success)
            {
                // Broadcast to group
                await Clients.Group($"support_conversation_{conversationId}").SendAsync("ReceiveMessage", new 
                {
                    conversationId = conversationId,
                    senderUserId = userId,
                    senderDisplayName = Context.User.Identity?.Name ?? "Unknown",
                    senderRole = isEmployee ? "Employee" : "Customer",
                    messageText = messageText,
                    createdAt = DateTime.UtcNow,
                    attachmentUrl = attachmentUrl,
                    isInternalNote = false
                });
            }
        }

        public async Task SendInternalNote(int conversationId, string messageText)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) return;

            bool isAdmin = Context.User.IsInRole("Admin");
            bool isEmployee = Context.User.IsInRole("Employee") || isAdmin;

            if (!isEmployee) return;
            if (string.IsNullOrWhiteSpace(messageText)) return;

            await _supportManager.AddInternalNoteAsync(userId, conversationId, messageText);

            // Broadcast to group but only for employees
            // In a real app, we might use a separate group for employees, 
            // but for now we'll broadcast and handle it in the client by role.
            await Clients.Group($"support_conversation_{conversationId}").SendAsync("ReceiveMessage", new
            {
                conversationId = conversationId,
                senderUserId = userId,
                senderDisplayName = Context.User.Identity?.Name ?? "Unknown",
                senderRole = "Employee",
                messageText = messageText,
                createdAt = DateTime.UtcNow,
                isInternalNote = true
            });
        }
    }
}
