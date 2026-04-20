using FinVentoryAPI.DTOs.SalesReportDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface ISalesReportService
    {
        Task<SalesReportResponseDto> GenerateAsync(SalesReportRequestDto req);
        Task<SalesReportFilterOptionsDto> GetFilterOptionsAsync();
    }
}
