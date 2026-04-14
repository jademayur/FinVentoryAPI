using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Services.Implementations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockService _service;
        private readonly IStockLedgerService _stockLedgerService;

        public StockController(IStockService service, IStockLedgerService stockLedgerService)
        {
            _service = service;
            _stockLedgerService = stockLedgerService;
        }

        [HttpPost("paged")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
            => Ok(await _service.GetPagedAsync(request));

        [HttpGet("groups")]
        public async Task<IActionResult> GetGroups()
            => Ok(await _service.GetGroupsAsync());

        [HttpGet("{itemId}/ledger")]
        public async Task<IActionResult> GetLedger(
    int itemId,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to)
        {
            var result = await _stockLedgerService.GetLedgerByItemAsync(itemId, from, to);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
