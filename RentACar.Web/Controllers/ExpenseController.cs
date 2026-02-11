using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs.Expense;
using RentACar.Application.Managers;

namespace RentACar.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ExpenseController : Controller
{
    private readonly ExpenseManager _manager;

    public ExpenseController(ExpenseManager manager)
    {
        _manager = manager;
    }

    public IActionResult Index()
    {
        return View("Index"); // Should leverage generic view logic or explicit view path
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        ViewBag.ExpenseId = id;
        return View();
    }
    
    // API Endpoints
    [HttpGet("api/Expense")]
    public async Task<ActionResult<ExpenseResultDto>> GetAll([FromQuery] ExpenseFilterDto filter)
    {
        var result = await _manager.GetExpensesAsync(filter);
        return Ok(result);
    }

    [HttpGet("api/Expense/{id}")]
    public async Task<ActionResult<ExpenseDetailsDto>> GetById(int id)
    {
        var expense = await _manager.GetExpenseDetailsAsync(id);
        if (expense == null) return NotFound();
        return Ok(expense);
    }

    [HttpPost("api/Expense")]
    public async Task<ActionResult<int>> Create([FromBody] ExpenseDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var id = await _manager.CreateExpenseAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("api/Expense/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ExpenseDto dto)
    {
        if (id != dto.ExpenseId) return BadRequest("ID mismatch");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _manager.UpdateExpenseAsync(dto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("api/Expense/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _manager.DeleteExpenseAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("api/Expense/generate-salaries")]
    public async Task<ActionResult<int>> GenerateSalaries()
    {
        var count = await _manager.GenerateMonthlySalariesAsync();
        return Ok(new { count });
    }
}
