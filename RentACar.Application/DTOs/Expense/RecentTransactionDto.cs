using System;

namespace RentACar.Application.DTOs.Expense;

public class RecentTransactionDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string Type { get; set; } = string.Empty; // "Income" or "Expense"
    public string Status { get; set; } = string.Empty;
}
