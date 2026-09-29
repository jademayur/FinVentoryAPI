using FinVentoryAPI.DTOs.ApprovalDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApprovalController : ControllerBase
    {
        private readonly IApprovalService _approvalService;

        public ApprovalController(IApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        // ════════════════════════════════════════════════════
        // GET LEVELS for a document type
        // ════════════════════════════════════════════════════
        [HttpGet("levels/{documentType}")]
        public async Task<IActionResult> GetLevels(string documentType)
        {
            var result = await _approvalService.GetLevelsAsync(documentType);
            return Ok(result);
        }

        // ════════════════════════════════════════════════════
        // UPSERT LEVELS
        // ════════════════════════════════════════════════════
        [HttpPost("levels")]
        public async Task<IActionResult> UpsertLevels([FromBody] BulkUpsertApprovalLevelDto dto)
        {
            await _approvalService.UpsertLevelsAsync(dto);
            return Ok(new { message = "Approval levels updated successfully." });
        }

        // ════════════════════════════════════════════════════
        // SUBMIT FOR APPROVAL
        // ════════════════════════════════════════════════════
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitForApproval([FromBody] ApprovalActionDto dto)
        {
            var result = await _approvalService.SubmitForApprovalAsync(dto.DocumentType, dto.DocumentId);
            return Ok(new { message = "Document submitted for approval.", success = result });
        }

        // ════════════════════════════════════════════════════
        // APPROVE
        // ════════════════════════════════════════════════════
        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] ApprovalActionDto dto)
        {
            var result = await _approvalService.ApproveAsync(dto.DocumentType, dto.DocumentId, dto.Remarks);
            return Ok(new { message = "Document approved successfully.", success = result });
        }

        // ════════════════════════════════════════════════════
        // REJECT
        // ════════════════════════════════════════════════════
        [HttpPost("reject")]
        public async Task<IActionResult> Reject([FromBody] ApprovalActionDto dto)
        {
            var result = await _approvalService.RejectAsync(dto.DocumentType, dto.DocumentId, dto.Remarks);
            return Ok(new { message = "Document rejected.", success = result });
        }

        // ════════════════════════════════════════════════════
        // GET PENDING APPROVALS
        // ════════════════════════════════════════════════════
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingApprovals([FromQuery] string? documentType)
        {
            var result = await _approvalService.GetPendingApprovalsAsync(documentType);
            return Ok(result);
        }

        // ════════════════════════════════════════════════════
        // GET HISTORY
        // ════════════════════════════════════════════════════
        [HttpGet("history/{documentType}/{documentId}")]
        public async Task<IActionResult> GetHistory(string documentType, int documentId)
        {
            var result = await _approvalService.GetHistoryAsync(documentType, documentId);
            return Ok(result);
        }

        // ════════════════════════════════════════════════════
        // CHECK IF APPROVAL REQUIRED
        // ════════════════════════════════════════════════════
        [HttpGet("is-required/{documentType}")]
        public async Task<IActionResult> IsApprovalRequired(string documentType)
        {
            var companyId = int.Parse(User.FindFirst("CompanyId")?.Value ?? "0");
            var result = await _approvalService.IsApprovalRequiredAsync(companyId, documentType);
            return Ok(new { required = result });
        }
    }
}
