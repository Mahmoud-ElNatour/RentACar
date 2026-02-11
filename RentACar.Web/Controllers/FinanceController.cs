using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentACar.Application.DTOs.Expense;
using RentACar.Application.Managers;

namespace RentACar.Web.Controllers;

[Authorize(Roles = "Admin")]
public class FinanceController : Controller
{
    private readonly FinanceManager _manager;

    public FinanceController(FinanceManager manager)
    {
        _manager = manager;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("api/Finance/Summary")]
    public async Task<ActionResult<FinanceSummaryDto>> GetSummary(DateOnly? startDate, DateOnly? endDate)
    {
        var summary = await _manager.GetFinanceSummaryAsync(startDate, endDate);
        return Ok(summary);
    }
}
