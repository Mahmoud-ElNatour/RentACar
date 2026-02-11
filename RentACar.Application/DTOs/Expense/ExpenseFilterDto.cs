using System;
using System.Collections.Generic;

namespace RentACar.Application.DTOs.Expense;

public class ExpenseFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public string? Status { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; }
}

public class ExpenseResultDto
{
    public List<ExpenseListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public ExpenseStatsDto? Stats { get; set; }
}

public class ExpenseStatsDto
{
    public decimal TotalPaidExpenses { get; set; }
    public decimal TotalPlannedExpenses { get; set; }
    public decimal TotalCancelledExpenses { get; set; }
    public int PaidCount { get; set; }
    public int PlannedCount { get; set; }
    public int CancelledCount { get; set; }
}
