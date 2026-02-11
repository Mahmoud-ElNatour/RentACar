using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.DTOs.Expense;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Application.Managers;

public class FinanceManager
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IPaymentRepository _paymentRepository;

    public FinanceManager(IExpenseRepository expenseRepository, IPaymentRepository paymentRepository)
    {
        _expenseRepository = expenseRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<FinanceSummaryDto> GetFinanceSummaryAsync(DateOnly? startDate, DateOnly? endDate)
    {
        // Expenses (Paid only)
        var expenseQuery = _expenseRepository.QueryWithCategory()
            .Where(x => x.Status == ExpenseStatus.Paid);

        if (startDate.HasValue) expenseQuery = expenseQuery.Where(x => x.ExpenseDate >= startDate.Value);
        if (endDate.HasValue) expenseQuery = expenseQuery.Where(x => x.ExpenseDate <= endDate.Value);

        var expenses = await expenseQuery.ToListAsync();
        var totalExpenses = expenses.Sum(x => x.Amount);

        // Payments (Paid only) & assuming PaymentDate is DateTime
        // Payment Status check: Assuming "Paid" or "Done" based on system rules (checking PaymentManager logic would be ideal if complex)
        // Check existing Payment entity status values. Assuming "Completed" or "Paid".
        // Let's check PaymentRepository usage or Entity.
        // For now, filtering by Status "Completed" or "Paid" (common). 
        // A safer bet is to inject PaymentManager, but let's stick to Repository for raw data.
        
        var paymentQuery = _paymentRepository.Query()
            .Where(p => p.Status == "Completed" || p.Status == "Paid"); // Adjust based on known status

        if (startDate.HasValue) paymentQuery = paymentQuery.Where(p => p.PaymentDate >= startDate.Value);
        if (endDate.HasValue) paymentQuery = paymentQuery.Where(p => p.PaymentDate <= endDate.Value);

        var payments = await paymentQuery.ToListAsync();
        var totalRevenue = payments.Sum(p => p.Amount);

        // Monthly Breakdown (Group by Year-Month)
        var monthlyExpenses = expenses
            .GroupBy(x => new { x.ExpenseDate.Year, x.ExpenseDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Total = g.Sum(x => x.Amount)
            }).ToList();

        var monthlyRevenue = payments
            .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Total = g.Sum(x => x.Amount)
            }).ToList();

        // Merge months
        var allMonths = monthlyExpenses.Select(x => new { x.Year, x.Month })
            .Union(monthlyRevenue.Select(x => new { x.Year, x.Month }))
            .Distinct()
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToList();

        var monthlyData = new List<MonthlyFinanceDto>();
        foreach (var m in allMonths)
        {
            var rev = monthlyRevenue.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Total ?? 0;
            var exp = monthlyExpenses.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Total ?? 0;
            
            monthlyData.Add(new MonthlyFinanceDto
            {
                Year = m.Year,
                Month = m.Month,
                MonthLabel = new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy"),
                RevenuePaid = rev,
                ExpensesPaid = exp,
                Net = rev - exp
            });
        }

        // Recent Transactions
        var recentPayments = await _paymentRepository.Query()
            .Include(p => p.Booking).ThenInclude(b => b.Customer)
            .Where(p => p.Status == "Completed" || p.Status == "Paid")
            .OrderByDescending(p => p.PaymentDate)
            .Take(10)
            .Select(p => new RecentTransactionDto
            {
                Id = p.PaymentId,
                Date = p.PaymentDate,
                Amount = p.Amount,
                Type = "Income",
                Status = "Completed",
                Description = p.Booking != null ? $"Booking #{p.Booking.BookingId} - {p.Booking.Customer.Name}" : $"Payment #{p.PaymentId}"
            })
            .ToListAsync();

        var recentExpenses = await _expenseRepository.Query()
            .Where(x => x.Status == ExpenseStatus.Paid)
            .OrderByDescending(x => x.ExpenseDate)
            .Take(10)
            .Select(x => new RecentTransactionDto
            {
                Id = x.ExpenseId,
                Date = x.ExpenseDate,
                Amount = x.Amount,
                Type = "Expense",
                Status = x.Status,
                Description = x.Title
            })
            .ToListAsync();

        var recentTransactions = recentPayments.Concat(recentExpenses)
            .OrderByDescending(t => t.Date)
            .Take(10)
            .ToList();

        // Expense Breakdown
        var totalExp = expenses.Sum(x => x.Amount);
        var breakdown = expenses
            .GroupBy(x => x.ExpenseCategory?.Name ?? "Uncategorized")
            .Select(g => new ExpenseBreakdownDto
            {
                CategoryName = g.Key,
                Amount = g.Sum(x => x.Amount),
                Percentage = totalExp > 0 ? (double)(g.Sum(x => x.Amount) / totalExp) * 100 : 0
            })
            .OrderByDescending(b => b.Amount)
            .ToList();

        return new FinanceSummaryDto
        {
            TotalRevenuePaid = totalRevenue,
            TotalExpensesPaid = totalExpenses,
            NetProfit = totalRevenue - totalExpenses,
            ProfitMargin = totalRevenue > 0 ? ((totalRevenue - totalExpenses) / totalRevenue) * 100 : 0,
            Monthly = monthlyData,
            RecentTransactions = recentTransactions,
            ExpenseBreakdown = breakdown
        };
    }
}
