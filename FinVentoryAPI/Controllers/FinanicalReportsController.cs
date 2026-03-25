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

    }
}
