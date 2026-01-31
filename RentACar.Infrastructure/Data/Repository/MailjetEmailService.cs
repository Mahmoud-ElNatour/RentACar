using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
<<<<<<< HEAD
using System.Linq;
=======
>>>>>>> Mahmoud-V3
using RentACar.Core.Repositories;

namespace RentACar.Infrastructure.Data.Repository
{
    public class MailjetEmailService : IEmailService
    {
        private readonly HttpClient _httpClient;

        public MailjetEmailService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

<<<<<<< HEAD
        public async Task SendEmailAsync(string toEmail, string subject, string message, System.Collections.Generic.Dictionary<string, byte[]> attachments = null, string? fromEmail = null, string? fromName = null)
=======
        public async Task SendEmailAsync(string toEmail, string subject, string message)
>>>>>>> Mahmoud-V3
        {
            var apiKey = Environment.GetEnvironmentVariable("MAILJET_API_KEY") ?? Environment.GetEnvironmentVariable("MAILJET_API_KEY", EnvironmentVariableTarget.User);
            var secretKey = Environment.GetEnvironmentVariable("MAILJET_SECRET_KEY") ?? Environment.GetEnvironmentVariable("MAILJET_SECRET_KEY", EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("Mailjet credentials are not set in Environment Variables.");
            }

            var payload = new
            {
                Messages = new[]
                {
                    new
                    {
                        From = new
                        {
<<<<<<< HEAD
                            Email = !string.IsNullOrEmpty(fromEmail) ? fromEmail : "info@rentacarmohammadmahmoud.shop",
                            Name = !string.IsNullOrEmpty(fromName) ? fromName : "Rent A Car"
=======
                            Email = "info@rentacarmohammadmahmoud.shop",
                            Name = "Rent A Car"
>>>>>>> Mahmoud-V3
                        },
                        To = new[]
                        {
                            new
                            {
                                Email = toEmail,
                                Name = "Customer"
                            }
                        },
                        Subject = subject,
<<<<<<< HEAD
                        HTMLPart = message,
                        Attachments = attachments != null ? attachments.Select(a => new
                        {
                            ContentType = "application/octet-stream", // Fallback, ideally we pass filename/type
                            Filename = a.Key,
                            Base64Content = Convert.ToBase64String(a.Value)
                        }).ToArray() : null
=======
                        HTMLPart = message
>>>>>>> Mahmoud-V3
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var authenticationString = $"{apiKey}:{secretKey}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mailjet.com/v3.1/send");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
