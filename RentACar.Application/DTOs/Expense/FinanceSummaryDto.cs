using System.Collections.Generic;

namespace RentACar.Application.DTOs.Expense;

public class FinanceSummaryDto
{
    public decimal TotalRevenuePaid { get; set; }
    public decimal TotalExpensesPaid { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    
    public List<MonthlyFinanceDto> Monthly { get; set; } = new();
    public List<RecentTransactionDto> RecentTransactions { get; set; } = new();
    public List<ExpenseBreakdownDto> ExpenseBreakdown { get; set; } = new();
}

public class ExpenseBreakdownDto
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percentage { get; set; }
}


public class MonthlyFinanceDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public decimal RevenuePaid { get; set; }
    public decimal ExpensesPaid { get; set; }
    public decimal Net { get; set; }
}
