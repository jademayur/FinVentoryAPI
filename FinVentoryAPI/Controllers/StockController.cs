using FinVentoryAPI.DTOs.PagedRequestDto;
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
        public StockController(IStockService service) => _service = service;

        [HttpPost("paged")]
        public async Task<IActionResult> GetPaged([FromBody] PagedRequestDto request)
            => Ok(await _service.GetPagedAsync(request));

        [HttpGet("groups")]
        public async Task<IActionResult> GetGroups()
            => Ok(await _service.GetGroupsAsync());
    }
}
