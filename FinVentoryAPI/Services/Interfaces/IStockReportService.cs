using FinVentoryAPI.DTOs.StockReportDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IStockReportService
    {
        Task<StockReportResponseDto> GenerateAsync(StockReportRequestDto req);
        Task<StockReportFilterOptionsDto> GetFilterOptionsAsync();
    }
}
