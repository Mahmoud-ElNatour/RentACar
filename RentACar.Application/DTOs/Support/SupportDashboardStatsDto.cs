using System;

namespace RentACar.Application.DTOs.Support
{
    public class SupportDashboardStatsDto
    {
        public int OpenTicketsCount { get; set; }
        public int WaitingForCustomerCount { get; set; }
        public int ResolvedTodayCount { get; set; }
        
        // Percentages/Trends (optional for UI)
        public double OpenTrend { get; set; }
        public double WaitingTrend { get; set; }
        public double ResolvedTrend { get; set; }
    }
}
