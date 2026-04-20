using FinVentoryAPI.DTOs.SalesReportDTOs;
using FinVentoryAPI.Services.Implementations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesReportController : ControllerBase
    {
        private readonly ISalesReportService _svc;
        public SalesReportController(ISalesReportService svc) => _svc = svc;

        // POST api/sales-report/generate
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] SalesReportRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.ReportType))
                return BadRequest(new { message = "ReportType is required." });

            try
            {
                var result = await _svc.GenerateAsync(req);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET api/sales-report/filters
        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var result = await _svc.GetFilterOptionsAsync();
            return Ok(result);
        }
    }
}
