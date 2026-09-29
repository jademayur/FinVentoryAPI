using FinVentoryAPI.DTOs.OutstandingReportDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IOutstandingReportService
    {
        Task<OutstandingReportResponseDto> GenerateAsync(OutstandingReportRequestDto req);
        Task<OutstandingReportFilterOptionsDto> GetFilterOptionsAsync(string partyType);
    }
}
