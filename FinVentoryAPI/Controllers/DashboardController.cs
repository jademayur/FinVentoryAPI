// FinVentoryAPI/Controllers/DashboardController.cs
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service)
        {
            _service = service;
        }

        [HttpGet("today-summary")]
        public async Task<IActionResult> GetTodaySummary()
        {
            var result = await _service.GetTodaySummaryAsync();
            return Ok(result);
        }

        [HttpGet("monthly-trend")]
        public async Task<IActionResult> GetMonthlyTrend([FromQuery] int months = 12)
        {
            var result = await _service.GetMonthlyTrendAsync(months);
            return Ok(result);
        }

        [HttpGet("overdue-receivables")]
        public async Task<IActionResult> GetOverdueReceivables()
        {
            var result = await _service.GetOverdueReceivablesAsync();
            return Ok(result);
        }

        [HttpGet("overdue-payables")]
        public async Task<IActionResult> GetOverduePayables()
        {
            var result = await _service.GetOverduePayablesAsync();
            return Ok(result);
        }

        [HttpGet("cash-bank-balances")]
        public async Task<IActionResult> GetCashBankBalances()
        {
            var result = await _service.GetCashBankBalancesAsync();
            return Ok(result);
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock()
        {
            var result = await _service.GetLowStockItemsAsync();
            return Ok(result);
        }

        [HttpGet("pending-docs")]
        public async Task<IActionResult> GetPendingDocs()
        {
            var result = await _service.GetPendingDocsAsync();
            return Ok(result);
        }
    }
}