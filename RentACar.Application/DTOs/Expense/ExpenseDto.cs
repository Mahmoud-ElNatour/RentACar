using System;

namespace RentACar.Application.DTOs.Expense;

public class ExpenseDto
{
    public int ExpenseId { get; set; }
    public int ExpenseCategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Vendor { get; set; }
    public string? ReferenceNumber { get; set; }
}

public class ExpenseListDto
{
    public int ExpenseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ExpenseDetailsDto : ExpenseDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string? CreatedByUserName { get; set; }
}
