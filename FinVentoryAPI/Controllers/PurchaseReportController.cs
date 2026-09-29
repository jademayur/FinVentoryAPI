using FinVentoryAPI.DTOs.PurchaseReportDTOs;
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
    public class PurchaseReportController : ControllerBase
    {
        private readonly IPurchaseReportService _svc;
        public PurchaseReportController(IPurchaseReportService svc) => _svc = svc;

        // POST api/purchase-report/generate
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] PurchaseReportRequestDto req)
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

        // GET api/purchase-report/filters
        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var result = await _svc.GetFilterOptionsAsync();
            return Ok(result);
        }
    }
}
