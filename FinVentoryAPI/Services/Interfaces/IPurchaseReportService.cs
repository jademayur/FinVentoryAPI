using FinVentoryAPI.DTOs.PurchaseReportDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IPurchaseReportService
    {
        Task<PurchaseReportResponseDto> GenerateAsync(PurchaseReportRequestDto req);
        Task<PurchaseReportFilterOptionsDto> GetFilterOptionsAsync();
    }
}
