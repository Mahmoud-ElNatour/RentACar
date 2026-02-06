using System.Threading.Tasks;
using RentACar.Application.DTOs.Support;
using System.Collections.Generic;

namespace RentACar.Application.Services
{
    public interface IGeminiAgentService
    {
        Task<string> GenerateResponseAsync(string userMessage, AiSupportContext context, List<string>? conversationHistory = null);
    }
}
