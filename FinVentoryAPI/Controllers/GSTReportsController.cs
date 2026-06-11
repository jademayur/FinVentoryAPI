using FinVentoryAPI.Services.Implementations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GSTReportsController : ControllerBase
    {
        private readonly IGSTReportsService _service;
        public GSTReportsController(IGSTReportsService gstReportsService)
        {
            _service = gstReportsService;
        }

        // ────────────────────────────────────────────────────────────────────
        // GET  api/gstr3b/summary?taxPeriod=04-2025
        // Returns the complete GSTR-3B summary for the given tax period
        // ────────────────────────────────────────────────────────────────────
        [HttpGet("gstr3b/summary")]
        public async Task<IActionResult> GetSummary([FromQuery] string taxPeriod)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(taxPeriod))
                    return BadRequest(new { message = "taxPeriod is required. Format: MM-YYYY" });

                var result = await _service.GetSummaryAsync(taxPeriod);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // GET  api/gstr3b/sales-invoices?taxPeriod=04-2025
        // Drill-down: all sales invoices contributing to the summary
        // ────────────────────────────────────────────────────────────────────
        [HttpGet("gstr3b/sales-invoices")]
        public async Task<IActionResult> GetSalesInvoices([FromQuery] string taxPeriod)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(taxPeriod))
                    return BadRequest(new { message = "taxPeriod is required. Format: MM-YYYY" });

                var result = await _service.GetSalesInvoicesAsync(taxPeriod);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // GET  api/gstr3b/purchase-invoices?taxPeriod=04-2025
        // Drill-down: all purchase invoices (ITC) for the period
        // ────────────────────────────────────────────────────────────────────
        [HttpGet("gstr3b/purchase-invoices")]
        public async Task<IActionResult> GetPurchaseInvoices([FromQuery] string taxPeriod)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(taxPeriod))
                    return BadRequest(new { message = "taxPeriod is required. Format: MM-YYYY" });

                var result = await _service.GetPurchaseInvoicesAsync(taxPeriod);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // GSTR-1  endpoints  ← NEW
        // ════════════════════════════════════════════════════════════════════

        // GET  api/gstreports/gstr1/summary?taxPeriod=04-2025
        [HttpGet("gstr1/summary")]
        public async Task<IActionResult> GetGstr1Summary([FromQuery] string taxPeriod)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(taxPeriod))
                    return BadRequest(new { message = "taxPeriod is required. Format: MM-YYYY" });

                var result = await _service.GetGstr1SummaryAsync(taxPeriod);
                return Ok(result);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // GET  api/gstreports/gstr1/b2b?taxPeriod=04-2025
        [HttpGet("gstr1/b2b")]
        public async Task<IActionResult> GetGstr1B2B([FromQuery] string taxPeriod)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(taxPeriod))
                    return BadRequest(new { message = "taxPeriod is required. Format: MM-YYYY" });

                var result = await _service.GetGstr1B2BAsync(taxPeriod);
                return Ok(result);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ════════════════════════════════════════════════════════════════════
        // GSTR-9  endpoints  ← NEW
        // ════════════════════════════════════════════════════════════════════

        // GET  api/gstreports/gstr9/summary?year=2024
        // year = start year of the financial year (2024 = FY 2024-25)
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
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // GET  api/gstreports/gstr9/monthly-breakdown?year=2024
        // Month-wise sales breakdown for drill-down
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
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("gstr1/doc-series")]
        public async Task<IActionResult> GetGstr1DocSeries([FromQuery] string taxPeriod)
        {
            try
            {
                var result = await _service.GetGstr1DocSeriesAsync(taxPeriod);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// GSTR-1 Table 12 — HSN-wise Summary
        /// GET /api/gst-reports/gstr1/hsn-summary?taxPeriod=04-2025
        /// </summary>
        [HttpGet("gstr1/hsn-summary")]
        public async Task<IActionResult> GetGstr1HsnSummary([FromQuery] string taxPeriod)
        {
            try
            {
                var result = await _service.GetGstr1HsnSummaryAsync(taxPeriod);
                return Ok(result);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>
        /// GSTR-1 Table 9B — CDNR (Credit/Debit Notes to Registered Dealers)
        /// GET /api/gst-reports/gstr1/cdnr?taxPeriod=04-2025
        /// </summary>
        [HttpGet("gstr1/cdnr")]
        public async Task<IActionResult> GetGstr1Cdnr([FromQuery] string taxPeriod)
        {
            try
            {
                var result = await _service.GetGstr1CdnrAsync(taxPeriod);
                return Ok(result);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>
        /// GSTR-1 Table 9B — CDNUR (Credit/Debit Notes to Unregistered Dealers)
        /// GET /api/gst-reports/gstr1/cdnur?taxPeriod=04-2025
        /// </summary>
        [HttpGet("gstr1/cdnur")]
        public async Task<IActionResult> GetGstr1Cdnur([FromQuery] string taxPeriod)
        {
            try
            {
                var result = await _service.GetGstr1CdnurAsync(taxPeriod);
                return Ok(result);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}
