using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentACar.Application.Managers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RentACar.Web.Services
{
    public class AiCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AiCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Run daily

        public AiCleanupService(IServiceProvider serviceProvider, ILogger<AiCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AI Cleanup Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("AI Cleanup Service running cleanup task...");
                    
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var aiManager = scope.ServiceProvider.GetRequiredService<AiManager>();
                        await aiManager.CleanupStaleConversationsAsync();
                    }
                    
                    _logger.LogInformation("AI Cleanup Service finished cleanup task.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing AI Cleanup task.");
                }

                // Wait for next cycle
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("AI Cleanup Service stopping.");
        }
    }
}
