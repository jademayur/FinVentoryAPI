using FinVentoryAPI.Services.Implementations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinanicalReportsController : ControllerBase
    {
        private readonly IFinancialReportService _service;
        public FinanicalReportsController(IFinancialReportService service)
        {
            _service = service;
        }

        [HttpGet("opening-trial-balance")]
        public async Task<IActionResult> GetOpeningTrialBalance()
        {
            var result = await _service.GetOpeningTrialBalanceAsync();
            return Ok(result);
        }

        [HttpGet("trial-balance/as-on-date")]
        public async Task<IActionResult> GetAsOnDateTrialBalance([FromQuery] DateTime asOnDate)
        {
            var result = await _service.GetAsOnDateTrialBalanceAsync(asOnDate);
            return Ok(result);
        }

        [HttpGet("trading")]
        public async Task<IActionResult> GetTrading([FromQuery] DateTime asOnDate)
    => Ok(await _service.GetTradingAccountAsync(asOnDate));

        [HttpGet("profit-loss")]
        public async Task<IActionResult> GetProfitLoss([FromQuery] DateTime asOnDate)
            => Ok(await _service.GetProfitAndLossAsync(asOnDate));

        [HttpGet("balance-sheet")]
        public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime asOnDate)
            => Ok(await _service.GetBalanceSheetAsync(asOnDate));

        

        [HttpGet("account-ledger/{accountId}")]
        public async Task<IActionResult> GetAccountLedger(
            int accountId,
            [FromQuery] DateTime asOnDate)
        {
            try
            {
                var result = await _service.GetAccountLedgerAsync(accountId, asOnDate);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
