using FinVentoryAPI.DTOs.OutstandingReportDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OutstandingReportController : ControllerBase
    {
        private readonly IOutstandingReportService _svc;
        public OutstandingReportController(IOutstandingReportService svc) => _svc = svc;

        // POST api/outstandingreport/generate
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] OutstandingReportRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.ReportType))
                return BadRequest(new { message = "ReportType is required." });

            if (string.IsNullOrWhiteSpace(req.PartyType))
                return BadRequest(new { message = "PartyType is required." });

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

        // GET api/outstandingreport/filters?partyType=Customer
        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters([FromQuery] string partyType = "Customer")
        {
            var result = await _svc.GetFilterOptionsAsync(partyType);
            return Ok(result);
        }
    }
}
