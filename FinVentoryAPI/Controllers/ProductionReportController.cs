using FinVentoryAPI.DTOs.ProductionReportDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductionReportController : ControllerBase
    {
        private readonly IProductionReportService _svc;
        public ProductionReportController(IProductionReportService svc) => _svc = svc;

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] ProductionReportRequestDto req)
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

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var result = await _svc.GetFilterOptionsAsync();
            return Ok(result);
        }
    }
}
