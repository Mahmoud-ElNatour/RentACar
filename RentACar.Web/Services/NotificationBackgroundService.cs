using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.Managers;
using RentACar.Infrastructure.Data;

namespace RentACar.Web.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(IServiceProvider serviceProvider, ILogger<NotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                int delayMinutes = 60; // Default

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var processingService = scope.ServiceProvider.GetRequiredService<NotificationProcessingService>();
                        
                        // 1. Get Settings to determine Interval
                        var settings = await processingService.GetSettingsAsync();
                        delayMinutes = settings.CheckIntervalMinutes > 0 ? settings.CheckIntervalMinutes : 60;

                        // 2. Run Logic
                        _logger.LogInformation($"Notification Service running 'System' trigger. Interval: {delayMinutes}m");
                        
                        // RunOnceAsync handles "Paused" logic internally (skips logic, logs "Skipped")
                        // OR we check here to avoid overhead? 
                        // ProcessingService.RunOnceAsync does check Enabled/Paused and logs it.
                        // So just calling it is fine and safe.
                        await processingService.RunOnceAsync("System");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Notification Background Service.");
                }

                // Wait for next interval
                await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);
            }
        }
    }
}
