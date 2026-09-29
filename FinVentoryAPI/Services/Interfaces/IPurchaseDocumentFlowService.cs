using FinVentoryAPI.DTOs.PurchaseDocumentFlowDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IPurchaseDocumentFlowService
    {
        Task<PurchaseDocumentFlowResponseDto> GetFlowAsync(PurchaseDocumentFlowRequestDto req);
    }
}
