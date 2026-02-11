using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RentACar.Application.DTOs.Expense;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Application.Managers;

public class ExpenseCategoryManager
{
    private readonly IExpenseCategoryRepository _repository;

    public ExpenseCategoryManager(IExpenseCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ExpenseCategoryDto>> GetActiveCategoriesAsync()
    {
        var categories = await _repository.GetActiveAsync();
        return categories.Select(c => new ExpenseCategoryDto
        {
            ExpenseCategoryId = c.ExpenseCategoryId,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive
        }).ToList();
    }

    public async Task<List<ExpenseCategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _repository.GetAllAsync();
        return categories.Select(c => new ExpenseCategoryDto
        {
            ExpenseCategoryId = c.ExpenseCategoryId,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive
        }).ToList();
    }

    public async Task<int> CreateCategoryAsync(ExpenseCategoryCreateDto dto)
    {
        if (await _repository.ExistsByNameAsync(dto.Name))
        {
            throw new InvalidOperationException($"Category '{dto.Name}' already exists.");
        }

        var category = new ExpenseCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(category);
        return created.ExpenseCategoryId;
    }

    public async Task UpdateCategoryAsync(ExpenseCategoryUpdateDto dto)
    {
        var category = await _repository.GetByIdAsync(dto.ExpenseCategoryId);
        if (category == null) throw new KeyNotFoundException("Category not found");

        if (category.Name != dto.Name && await _repository.ExistsByNameAsync(dto.Name))
        {
             throw new InvalidOperationException($"Category '{dto.Name}' already exists.");
        }

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.IsActive = dto.IsActive;

        await _repository.UpdateAsync(category);
    }

    public async Task ToggleActiveAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null) throw new KeyNotFoundException("Category not found");

        category.IsActive = !category.IsActive;
        await _repository.UpdateAsync(category);
    }
}
