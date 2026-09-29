using FinVentoryAPI.DTOs.ProductionReportDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IProductionReportService
    {
        Task<ProductionReportResponseDto> GenerateAsync(ProductionReportRequestDto req);
        Task<ProductionReportFilterOptionsDto> GetFilterOptionsAsync();
    }
}
