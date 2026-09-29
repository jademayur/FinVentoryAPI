using FinVentoryAPI.DTOs.ApprovalDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IApprovalService
    {
        Task<List<ApprovalLevelDto>> GetLevelsAsync(string documentType);
        Task UpsertLevelsAsync(BulkUpsertApprovalLevelDto dto);
        Task<bool> IsApprovalRequiredAsync(int companyId, string documentType);
        Task<bool> SubmitForApprovalAsync(string documentType, int documentId);
        Task<bool> ApproveAsync(string documentType, int documentId, string? remarks);
        Task<bool> RejectAsync(string documentType, int documentId, string remarks);
        Task<List<ApprovalQueueDto>> GetPendingApprovalsAsync(string? documentType);
        Task<ApprovalHistoryDto> GetHistoryAsync(string documentType, int documentId);
        Task<string?> GetCurrentApprovalStatusAsync(int companyId, string documentType, int documentId);
    }
}
