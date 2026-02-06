using Microsoft.AspNetCore.SignalR;
using RentACar.Application.Services;
using RentACar.Web.Hubs;
using System.Threading.Tasks;

namespace RentACar.Web.Services
{
    public class SignalRBroadcaster : ISignalRBroadcaster
    {
        private readonly IHubContext<SupportChatHub> _hubContext;

        public SignalRBroadcaster(IHubContext<SupportChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task BroadcastSupportMessageAsync(int conversationId, object messageDto)
        {
            if (messageDto is RentACar.Application.DTOs.Support.SupportMessageDto dto)
            {
               // Match the anonymous object structure used in SupportChatHub.SendMessage and client expectations
               await _hubContext.Clients.Group($"support_conversation_{conversationId}").SendAsync("ReceiveMessage", new 
               {
                    conversationId = dto.ConversationId,
                    senderUserId = dto.SenderUserId,
                    senderDisplayName = dto.SenderDisplayName ?? "AI Assistant",
                    senderRole = dto.SenderRole,
                    messageText = dto.MessageText,
                    createdAt = dto.CreatedAt,
                    attachmentUrl = dto.AttachmentUrl,
                    isInternalNote = dto.IsInternalNote
               });
            }
        }
    }
}
