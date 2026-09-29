using FinVentoryAPI.DTOs.PurchaseDocumentFlowDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseDocumentFlowController : ControllerBase
    {
        private readonly IPurchaseDocumentFlowService _svc;
        public PurchaseDocumentFlowController(IPurchaseDocumentFlowService svc) => _svc = svc;

        [HttpPost("list")]
        public async Task<IActionResult> GetFlow([FromBody] PurchaseDocumentFlowRequestDto req)
        {
            try
            {
                var result = await _svc.GetFlowAsync(req);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
