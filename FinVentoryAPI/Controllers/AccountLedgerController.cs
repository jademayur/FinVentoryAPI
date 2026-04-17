using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountLedgerController : ControllerBase
    {
        private readonly IAccountLedgerService _service;

        public AccountLedgerController(IAccountLedgerService service)
        {
            _service = service;
        }

        // GET /api/accountledger/{accountId}?from=2024-04-01&to=2025-03-31
        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetByAccount(
            int accountId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var result = await _service.GetLedgerByAccountAsync(accountId, from, to);

            if (result == null)
                return NotFound(new { message = "Account not found." });

            return Ok(result);
        }

        // GET /api/accountledger?from=2024-04-01&to=2025-03-31&accountGroupId=1
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? accountGroupId)
        {
            var result = await _service.GetAllLedgersAsync(from, to, accountGroupId);
            return Ok(result);
        }
    }
}
