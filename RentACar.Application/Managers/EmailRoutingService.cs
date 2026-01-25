using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Entities;
using RentACar.Infrastructure.Data;

namespace RentACar.Application.Managers
{
    public class EmailRoutingService
    {
        private readonly ApplicationDbContext _context;

        public EmailRoutingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public class RoutingResult
        {
            public SenderIdentity Sender { get; set; }
            public string TemplateKey { get; set; }
            public bool IsEnabled { get; set; }
        }

        public async Task<RoutingResult> ResolveRouteAsync(string featureKey)
        {
            var config = await _context.EmailFeatureConfigs
                .Include(c => c.SenderIdentity)
                .FirstOrDefaultAsync(c => c.FeatureKey == featureKey);

            // Default fallback if not found or no sender configured
            var defaultSender = await _context.SenderIdentities.FirstOrDefaultAsync(s => s.IsDefault) 
                                ?? await _context.SenderIdentities.FirstOrDefaultAsync();

            if (config == null)
            {
                // Feature not in DB? Treat as enabled default (or disabled? Safe to say enabled with default sender)
                return new RoutingResult 
                { 
                    IsEnabled = true, 
                    Sender = defaultSender, 
                    TemplateKey = null // Logic downstream should decide default content if null
                };
            }

            if (!config.Enabled) return new RoutingResult { IsEnabled = false };

            return new RoutingResult
            {
                IsEnabled = true,
                Sender = config.SenderIdentity ?? defaultSender,
                TemplateKey = config.TemplateKey
            };
        }
    }
}
