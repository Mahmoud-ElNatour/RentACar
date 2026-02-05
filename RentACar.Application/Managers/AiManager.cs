using Microsoft.EntityFrameworkCore;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Enums;
using RentACar.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentACar.Application.Managers
{
    public class AiManager
    {
        private readonly RentACarDbContext _context;

        public AiManager(RentACarDbContext context)
        {
            _context = context;
        }

        public async Task<AiConversation> GetOrCreateConversationAsync(int customerId)
        {
            var conversation = await _context.AiConversations
                .Include(c => c.Messages)
                .Where(c => c.CustomerId == customerId && !c.IsEscalated)
                .OrderByDescending(c => c.LastActiveAt)
                .FirstOrDefaultAsync();

            if (conversation == null)
            {
                conversation = new AiConversation
                {
                    CustomerId = customerId,
                    CreatedAt = DateTime.UtcNow,
                    LastActiveAt = DateTime.UtcNow
                };
                _context.AiConversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            return conversation;
        }

        public async Task SaveMessageAsync(int conversationId, string content, AiSenderType sender)
        {
            var conversation = await _context.AiConversations.FindAsync(conversationId);
            if (conversation == null) return;

            var message = new AiMessage
            {
                AiConversationId = conversationId,
                Sender = sender,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            conversation.LastActiveAt = DateTime.UtcNow;
            
            _context.AiMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AiMessage>> GetHistoryAsync(int conversationId)
        {
            return await _context.AiMessages
                .Where(m => m.AiConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task CleanupStaleConversationsAsync()
        {
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            // 1. Delete Non-Escalated older than 7 days
            var staleDrafts = await _context.AiConversations
                .Where(c => !c.IsEscalated && c.LastActiveAt < sevenDaysAgo)
                .ToListAsync();

            if (staleDrafts.Any())
            {
                _context.AiConversations.RemoveRange(staleDrafts);
            }

            // 2. Delete Escalated older than 30 days (Privacy/Cleanup)
            var oldEscalated = await _context.AiConversations
                .Where(c => c.IsEscalated && c.CreatedAt < thirtyDaysAgo)
                .ToListAsync();

            if (oldEscalated.Any())
            {
                _context.AiConversations.RemoveRange(oldEscalated);
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarkEscalatedAsync(int conversationId)
        {
            var conversation = await _context.AiConversations.FindAsync(conversationId);
            if (conversation != null)
            {
                conversation.IsEscalated = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
