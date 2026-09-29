using FinVentoryAPI.DTOs.SalesPipelineDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface ISalesDocumentFlowService
    {
        Task<SalesDocumentFlowResponseDto> GetPipelineAsync(SalesDocumentFlowRequestDto req);
    }
}
