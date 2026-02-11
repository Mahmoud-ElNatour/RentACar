using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentACar.Application.DTOs.Expense;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Application.Managers;

public class ExpenseManager
{
    private readonly IExpenseRepository _repository;
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public ExpenseManager(IExpenseRepository repository, IExpenseCategoryRepository categoryRepository, IEmployeeRepository employeeRepository)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<ExpenseResultDto> GetExpensesAsync(ExpenseFilterDto filter)
    {
        var query = _repository.QueryWithCategory();

        // Filtering
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(term) 
                                     || (x.Vendor != null && x.Vendor.ToLower().Contains(term))
                                     || (x.ReferenceNumber != null && x.ReferenceNumber.ToLower().Contains(term)));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x => x.ExpenseCategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrEmpty(filter.Status))
        {
            query = query.Where(x => x.Status == filter.Status);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(x => x.ExpenseDate >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(x => x.ExpenseDate <= filter.EndDate.Value);
        }

        // Stats (calculated before paging)
        var stats = new ExpenseStatsDto
        {
            TotalPaidExpenses = await query.Where(x => x.Status == ExpenseStatus.Paid).SumAsync(x => x.Amount),
            TotalPlannedExpenses = await query.Where(x => x.Status == ExpenseStatus.Planned).SumAsync(x => x.Amount),
            TotalCancelledExpenses = await query.Where(x => x.Status == ExpenseStatus.Cancelled).SumAsync(x => x.Amount),
            PaidCount = await query.CountAsync(x => x.Status == ExpenseStatus.Paid),
            PlannedCount = await query.CountAsync(x => x.Status == ExpenseStatus.Planned),
            CancelledCount = await query.CountAsync(x => x.Status == ExpenseStatus.Cancelled)
        };

        // Sorting
        query = filter.SortColumn switch
        {
            "Amount" => filter.SortDirection == "asc" ? query.OrderBy(x => x.Amount) : query.OrderByDescending(x => x.Amount),
            "Date" => filter.SortDirection == "asc" ? query.OrderBy(x => x.ExpenseDate) : query.OrderByDescending(x => x.ExpenseDate),
            "Category" => filter.SortDirection == "asc" ? query.OrderBy(x => x.ExpenseCategory.Name) : query.OrderByDescending(x => x.ExpenseCategory.Name),
            "Status" => filter.SortDirection == "asc" ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
            _ => query.OrderByDescending(x => x.ExpenseDate) // Default
        };

        // Pagination
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
        var items = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

        var dtos = items.Select(x => new ExpenseListDto
        {
            ExpenseId = x.ExpenseId,
            Title = x.Title,
            CategoryName = x.ExpenseCategory.Name,
            Amount = x.Amount,
            ExpenseDate = x.ExpenseDate,
            Status = x.Status
        }).ToList();

        return new ExpenseResultDto
        {
            Items = dtos,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Stats = stats
        };
    }

    public async Task<ExpenseDetailsDto?> GetExpenseDetailsAsync(int id)
    {
        var expense = await _repository.GetByIdWithCategoryAsync(id);
        if (expense == null) return null;

        return new ExpenseDetailsDto
        {
            ExpenseId = expense.ExpenseId,
            ExpenseCategoryId = expense.ExpenseCategoryId,
            CategoryName = expense.ExpenseCategory.Name,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            Status = expense.Status,
            Title = expense.Title,
            Description = expense.Description,
            Vendor = expense.Vendor,
            ReferenceNumber = expense.ReferenceNumber,
            CreatedByUserName = expense.CreatedByUser?.UserName
        };
    }

    public async Task<int> CreateExpenseAsync(ExpenseDto dto, string? userId)
    {
        var expense = new Expense
        {
            ExpenseCategoryId = dto.ExpenseCategoryId,
            Amount = dto.Amount,
            ExpenseDate = dto.ExpenseDate,
            Status = dto.Status,
            Title = dto.Title,
            Description = dto.Description,
            Vendor = dto.Vendor,
            ReferenceNumber = dto.ReferenceNumber,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(expense);
        return created.ExpenseId;
    }

    public async Task UpdateExpenseAsync(ExpenseDto dto)
    {
        var expense = await _repository.GetByIdAsync(dto.ExpenseId);
        if (expense == null) throw new KeyNotFoundException("Expense not found");

        expense.ExpenseCategoryId = dto.ExpenseCategoryId;
        expense.Amount = dto.Amount;
        expense.ExpenseDate = dto.ExpenseDate;
        expense.Status = dto.Status;
        expense.Title = dto.Title;
        expense.Description = dto.Description;
        expense.Vendor = dto.Vendor;
        expense.ReferenceNumber = dto.ReferenceNumber;
        expense.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(expense);
    }

    public async Task DeleteExpenseAsync(int id)
    {
        var expense = await _repository.GetByIdAsync(id);
        if (expense == null) throw new KeyNotFoundException("Expense not found");
        await _repository.DeleteAsync(expense);
    }

    public async Task<int> GenerateMonthlySalariesAsync()
    {
        var employees = await _employeeRepository.Query()
            .Where(e => e.IsActive && e.Salary > 0)
            .ToListAsync();

        if (!employees.Any()) return 0;

        // Ensure "Salaries" category exists
        var category = await _categoryRepository.Query().FirstOrDefaultAsync(c => c.Name == "Salaries");
        if (category == null)
        {
            category = new ExpenseCategory { Name = "Salaries", IsActive = true };
            await _categoryRepository.AddAsync(category);
        }

        int generatedCount = 0;
        var currentMonth = DateTime.Now.ToString("yyyyMM"); 

        foreach (var emp in employees)
        {
            var refNum = $"SAL-{emp.EmployeeId}-{currentMonth}";
            
            // Check if exists
            var exists = await _repository.Query()
                .AnyAsync(x => x.ReferenceNumber == refNum);

            if (!exists)
            {
                var expense = new Expense
                {
                    ExpenseCategoryId = category.ExpenseCategoryId,
                    Title = $"Salary: {emp.Name}",
                    Description = $"Monthly salary for {DateTime.Now:MMMM yyyy}",
                    Amount = emp.Salary.Value,
                    ExpenseDate = DateOnly.FromDateTime(DateTime.Today),
                    Status = ExpenseStatus.Planned,
                    Vendor = "Internal",
                    ReferenceNumber = refNum,
                    CreatedByUserId = null, // System generated
                    CreatedAt = DateTime.UtcNow
                };
                await _repository.AddAsync(expense);
                generatedCount++;
            }
        }
        return generatedCount;
    }
}
