using FinVentoryAPI.DTOs.FinancialReportDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IFinancialReportService
    {
        Task<List<BalanceGroupDto>> GetOpeningTrialBalanceAsync();
    }
}
