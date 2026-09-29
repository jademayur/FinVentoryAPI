using FinVentoryAPI.Services.Implementations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GSTReportsController : ControllerBase
    {
        private readonly IGSTReportsService _service;
        public GSTReportsController(IGSTReportsService gstReportsService)
        {
            _service = gstReportsService;
        }

        // ────────────────────────────────────────────────────────────────────
        // GSTR-3B
        // ────────────────────────────────────────────────────────────────────
        [HttpGet("gstr3b/summary")]
        public async Task<IActionResult> GetSummary([FromQuery] string? dateFrom, [FromQuery] string? dateTo)
        {
            try
            {
                var result = await _service.GetSummaryAsync(dateFrom, dateTo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("gstr3b/sales-invoices")]
        public async Task<IActionResult> GetSalesInvoices([FromQuery] string? dateFrom, [FromQuery] string? dateTo)
        {
            try
            {
                var result = await _service.GetSalesInvoicesAsync(dateFrom, dateTo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("gstr3b/purchase-invoices")]
        public async Task<IActionResult> GetPurchaseInvoices([FromQuery] string? dateFrom, [FromQuery] string? dateTo)
        {
            try
            {
                var result = await _service.GetPurchaseInvoicesAsync(dateFrom, dateTo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // GSTR-1
        // ════════════════════════════════════════════════════════════════════
        [HttpGet("gstr1/summary")]
        public async Task<IActionResult> GetGstr1Summary([FromQuery] string? dateFrom, [FromQuery] string? dateTo)
        {
            try
            {
                var result = await _service.GetGstr1SummaryAsync(dateFrom, dateTo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("gstr1/b2b")]
        public async Task<IActionResult> GetGstr1B2B([FromQuery] string? dateFrom, [FromQuery] string? dateTo)
        {
            try
            {
                var result = await _service.GetGstr1B2BAsync(dateFrom, dateTo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("gstr1/hsn-summary")]
        public async Task<IActionResult> GetGstr1HsnSummary([FromQuery] string? dateFrom, [FromQuery] string? dateTo)
        {
            try
            {
                var result = await _service.GetGstr1HsnSummaryAsync(dateFrom, dateTo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("gstr1/cdnr")]
        public async Task<IActionResult> GetGstr1Cdnr([FromQuery] string? dateFrom, [FromQuery] string? dateTo)
        {
            try
            {
                var result = await _service.GetGstr1CdnrAsync(dateFrom, dateTo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("gstr1/cdnur")]
        public async Task<IActionResult> GetGstr1Cdnur([FromQuery] string? dateFrom, [FromQuery] string? dateTo)
        {
            try
            {
                var result = await _service.GetGstr1CdnurAsync(dateFrom, dateTo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("gstr1/doc-series")]
        public async Task<IActionResult> GetGstr1DocSeries([FromQuery] string? dateFrom, [FromQuery] string? dateTo)
        {
            try
            {
                var result = await _service.GetGstr1DocSeriesAsync(dateFrom, dateTo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // GSTR-9
        // ════════════════════════════════════════════════════════════════════
        [HttpGet("gstr9/summary")]
        public async Task<IActionResult> GetGstr9Summary([FromQuery] int year)
        {
            try
            {
                if (year < 2000 || year > 2100)
                    return BadRequest(new { message = "year is required. Example: 2024 for FY 2024-25" });

                var result = await _service.GetGstr9SummaryAsync(year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("gstr9/monthly-breakdown")]
        public async Task<IActionResult> GetGstr9MonthlyBreakdown([FromQuery] int year)
        {
            try
            {
                if (year < 2000 || year > 2100)
                    return BadRequest(new { message = "year is required. Example: 2024 for FY 2024-25" });

                var result = await _service.GetGstr9MonthlyBreakdownAsync(year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
