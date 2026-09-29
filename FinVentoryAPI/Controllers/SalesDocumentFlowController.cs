using FinVentoryAPI.DTOs.SalesPipelineDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalesDocumentFlowController : ControllerBase
    {
        private readonly ISalesDocumentFlowService _svc;
        public SalesDocumentFlowController(ISalesDocumentFlowService svc) => _svc = svc;

        [HttpPost("list")]
        public async Task<IActionResult> GetPipeline([FromBody] SalesDocumentFlowRequestDto req)
        {
            try
            {
                var result = await _svc.GetPipelineAsync(req);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
