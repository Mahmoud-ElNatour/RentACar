using System;

namespace RentACar.Application.DTOs
{
    public class PaymentFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; } // "asc" or "desc"
    }
}
