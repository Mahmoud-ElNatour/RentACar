using System.Collections.Generic;

namespace RentACar.Application.DTOs
{
    public class PaymentResultDto
    {
        public List<PaymentListDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public PaymentStatsDto Stats { get; set; } = new();
    }

    public class PaymentStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal PendingAmount { get; set; }
        public int PendingCount { get; set; }
        public int SuccessCount { get; set; }
        public int TotalCount { get; set; }
        public decimal RefundAmount { get; set; }
    }
}
