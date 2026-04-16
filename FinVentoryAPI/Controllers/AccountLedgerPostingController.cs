using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountLedgerPostingController : ControllerBase
    {
        private readonly IAccountLedgerPostingService _service;

        public AccountLedgerPostingController(IAccountLedgerPostingService service)
        {
            _service = service;
        }

        // GET api/AccountLedgerPosting/{accountId}/ledger?from=...&to=...
        [HttpGet("{accountId}/ledger")]
        public async Task<IActionResult> GetLedger(
            int accountId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var result = await _service.GetLedgerByAccountAsync(accountId, from, to);
            if (result == null) return NotFound(new { message = "Account not found." });
            return Ok(result);
        }

        // GET api/AccountLedgerPosting/all?from=...&to=...&accountGroupId=...
        [HttpGet("all")]
        public async Task<IActionResult> GetAllLedgers(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? accountGroupId)
        {
            var result = await _service.GetAllLedgersAsync(from, to, accountGroupId);
            return Ok(result);
        }

        // DELETE api/AccountLedgerPosting/{postingId}
        [HttpDelete("{postingId}")]
        public async Task<IActionResult> DeleteEntry(int postingId)
        {
            var result = await _service.DeleteEntryAsync(postingId);
            if (!result) return NotFound(new { message = "Posting entry not found." });
            return Ok(new { message = "Entry deleted successfully." });
        }
    }
}
