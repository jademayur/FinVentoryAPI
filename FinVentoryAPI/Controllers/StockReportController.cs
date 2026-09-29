using FinVentoryAPI.DTOs.StockReportDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockReportController : ControllerBase
    {
        private readonly IStockReportService _service;

        public StockReportController(IStockReportService service)
        {
            _service = service;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] StockReportRequestDto req)
        {
            try
            {
                var result = await _service.GenerateAsync(req);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            try
            {
                var result = await _service.GetFilterOptionsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
