using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs.Expense;
using RentACar.Application.Managers;

namespace RentACar.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class ExpenseCategoryController : ControllerBase
{
    private readonly ExpenseCategoryManager _manager;

    public ExpenseCategoryController(ExpenseCategoryManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public async Task<ActionResult<List<ExpenseCategoryDto>>> GetAll()
    {
        return Ok(await _manager.GetAllCategoriesAsync());
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<ExpenseCategoryDto>>> GetActive()
    {
        return Ok(await _manager.GetActiveCategoriesAsync());
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(ExpenseCategoryCreateDto dto)
    {
        try
        {
            var id = await _manager.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetAll), new { id }, id); // Ideally GetById
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ExpenseCategoryUpdateDto dto)
    {
        if (id != dto.ExpenseCategoryId) return BadRequest("ID mismatch");

        try
        {
            await _manager.UpdateCategoryAsync(dto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/toggle")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        try
        {
            await _manager.ToggleActiveAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
