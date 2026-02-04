using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RentACar.Application.DTOs.Support; // Using the new DTO namespace

namespace RentACar.Application.Services
{
    public class GeminiAgentService : IGeminiAgentService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GeminiAgentService> _logger;

        private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-001:generateContent";

        public GeminiAgentService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiAgentService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiChatbot:ApiKey"];
            _logger = logger;
        }

        public async Task<string> GenerateResponseAsync(string userMessage, AiSupportContext context, List<string>? conversationHistory = null)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("Gemini API Key is missing.");
                // Return a polite fallback if API is down/missing
                return "I apologize, but I am currently experiencing technical difficulties. Please try again later or contact support.";
            }

            try
            {
                var systemPrompt = BuildSystemPrompt(context, conversationHistory);
                var fullPrompt = $"{systemPrompt}\n\nUser Question: {userMessage}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = fullPrompt }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Construct URL with Key
                var url = $"{ApiUrl}?key={_apiKey}";

                var response = await _httpClient.PostAsync(url, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API Error: {StatusCode} - {Error}", response.StatusCode, error);
                    return "I apologize, but I am unable to process your request at the moment. Please try again later.";
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GeminiResponse>(responseString);

                var answer = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
                
                // Post-processing to enforce "Did this help?" rule if not present 
                // (Only add if answer is substantial)
                if (!string.IsNullOrEmpty(answer))
                {
                    if (!answer.Contains("Did this", StringComparison.OrdinalIgnoreCase) && 
                        !answer.Contains("help", StringComparison.OrdinalIgnoreCase) && 
                         answer.Length > 50) 
                    {
                        answer += "\n\nDid this answer your question? If not, you can ask to speak to a human agent.";
                    }
                }

                return answer ?? "I apologize, I received an empty response. Please try again.";

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception calling Gemini API");
                return "I apologize, but an error occurred while processing your request. Please try again later.";
            }
        }

        private string BuildSystemPrompt(AiSupportContext context, List<string>? history = null)
        {
            var sb = new StringBuilder();

            // --- 1. CONSTITUTION (RULES) ---
            sb.AppendLine("ROLE: You represent 'Lebanon Drive RentACar' as a helpful, READ-ONLY Customer Support AI.");
            sb.AppendLine("STRICT RULES:");
            sb.AppendLine("- YOU CANNOT CHANGE DATA. You are Read-Only.");
            sb.AppendLine("- REFUSE requests to: Cancel bookings, Refund payments, Change dates, Assign drivers.");
            sb.AppendLine("- RESPONSE PATTERN for refusals: 'I cannot directly [action]. Please contact a human agent for assistance.'");
            sb.AppendLine("- ANSWER CONFIDENTLY about: Car availability, Prices, Policies, User's own bookings/payments.");
            sb.AppendLine("- ESCALATE if uncertain.");

            // --- 2. GLOBAL BUSINESS CONTEXT ---
            sb.AppendLine("\n=== [GLOBAL BUSINESS CONTEXT] ===");
            
            sb.AppendLine("-- COMPANY CONTACT --");
            sb.AppendLine($"Email: {context.GlobalContext.Company.Email}");
            sb.AppendLine($"Phone: {context.GlobalContext.Company.PhoneNumber}");
            sb.AppendLine($"Address: {context.GlobalContext.Company.Address}");

            sb.AppendLine("\n-- INVENTORY SUMMARY --");
            if (context.GlobalContext.InventorySummary.Any())
                foreach(var item in context.GlobalContext.InventorySummary) sb.AppendLine($"- {item}");
            else
                sb.AppendLine("No specific inventory data available.");

            sb.AppendLine("\n-- CATEGORIES (Types of Cars) --");
            if (context.GlobalContext.AllCategories.Any())
            {
                sb.AppendLine($"Available Categories: {string.Join(", ", context.GlobalContext.AllCategories)}");
                sb.AppendLine("NOTE: A 'Category' (e.g. SUV, Sedan) group cars. A 'Model' is the specific car name (e.g. BMW X5).");
            }

            sb.AppendLine("\n-- ACTIVE PROMOTIONS --");
            if (context.GlobalContext.ActivePromotions.Any())
                foreach (var promo in context.GlobalContext.ActivePromotions) sb.AppendLine($"- {promo}");
            else
                sb.AppendLine("No active promotions.");

            sb.AppendLine("\n-- POLICIES --");
            sb.AppendLine($"Cancellation: {context.GlobalContext.Policies.CancellationPolicy}");
            sb.AppendLine($"Driver Req: {context.GlobalContext.Policies.DriverRequirements}");
            sb.AppendLine($"Security Deposit: {context.GlobalContext.Policies.SecurityDeposit}");
            sb.AppendLine($"Verification: {context.GlobalContext.Policies.VerificationSteps}");

            sb.AppendLine("\n-- PAYMENT METHODS --");
            sb.AppendLine($"Accepted: {string.Join(", ", context.GlobalContext.PaymentMethods)}");

            // --- 2b. REAL-TIME FLEET AVAILABILITY ---
            sb.AppendLine("\n=== [FLEET AVAILABILITY & CALENDAR (NEXT 45 DAYS)] ===");
            sb.AppendLine("Use this calendar to check car availability. If a car is listed as Booked during the user's requested dates, it is UNAVAILABLE.");
            
            if (context.GlobalContext.FleetAvailability.Any())
                foreach(var item in context.GlobalContext.FleetAvailability) sb.AppendLine($"- {item}");
            else
                sb.AppendLine("No availability data. Assume all cars are available unless stated otherwise.");

            sb.AppendLine("\n-- STATUS DEFINITIONS --");
            foreach(var kvp in context.GlobalContext.StatusDefinitions)
            {
                sb.AppendLine($"{kvp.Key}: {kvp.Value}");
            }

            // --- 3. USER PRIVATE CONTEXT ---
            sb.AppendLine($"\n=== [USER CONTEXT: {context.UserContext.Name}] ===");
            sb.AppendLine($"Verified: {context.UserContext.IsVerified}");
            sb.AppendLine($"Customer ID: {context.UserContext.CustomerId}");
            sb.AppendLine($"Email: {context.UserContext.Email}");
            sb.AppendLine($"Phone: {context.UserContext.PhoneNumber}");

            if (context.UserContext.ActiveBooking != null)
            {
                var b = context.UserContext.ActiveBooking;
                sb.AppendLine("\n-- **ACTIVE BOOKING** --");
                sb.AppendLine($"Booking ID: {b.BookingId} | Status: {b.Status}");
                sb.AppendLine($"Car: {b.CarName} ({b.PlateNumber}) - {b.Color}");
                sb.AppendLine($"Period: {b.StartDate:yyyy-MM-dd} to {b.EndDate:yyyy-MM-dd}");
                sb.AppendLine($"Pickup: {b.PickupDateTime:g} @ {b.PickupLocationLabel}");
                sb.AppendLine($"Total: {b.TotalPrice:C} | Driver: {(b.HasDriver ? "Yes" : "No")}");
            }

            if (context.UserContext.RecentBookings.Any())
            {
                 sb.AppendLine("\n-- RECENT HISTORY --");
                 foreach(var b in context.UserContext.RecentBookings)
                 {
                     sb.AppendLine($"ID {b.BookingId}: {b.CarName} ({b.Status}) - {b.StartDate:MM/dd}");
                 }
            }

            if (context.UserContext.RecentPayments.Any())
            {
                sb.AppendLine("\n-- RECENT PAYMENTS --");
                foreach (var p in context.UserContext.RecentPayments)
                {
                    sb.AppendLine($"Pay ID {p.PaymentId}: {p.Amount:C} ({p.Status}) via {p.Method} on {p.Date:MM/dd}");
                }
            }

            // --- 4. CONVERSATION MEMORY ---
            if (history != null && history.Any())
            {
                sb.AppendLine("\n=== [CONVERSATION HISTORY (Last 6 Messages)] ===");
                foreach(var msg in history)
                {
                    sb.AppendLine(msg);
                }
            }

            sb.AppendLine("\n--- END OF CONTEXT ---");
            sb.AppendLine("Answer the user's question using ONLY the above information. Do not hallucinate data not present here.");

            return sb.ToString();
        }
    }

    // Models for Gemini Response (Internal to this file)
    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; }
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
