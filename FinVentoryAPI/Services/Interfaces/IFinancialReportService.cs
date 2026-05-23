using FinVentoryAPI.DTOs.FinancialReportDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IFinancialReportService
    {
        Task<List<BalanceGroupDto>> GetOpeningTrialBalanceAsync();
        Task<List<BalanceGroupDto>> GetAsOnDateTrialBalanceAsync(DateTime asOnDate);
        Task<TradingAccountDto> GetTradingAccountAsync(DateTime asOnDate);
        Task<ProfitAndLossDto> GetProfitAndLossAsync(DateTime asOnDate);
        Task<BalanceSheetDto> GetBalanceSheetAsync(DateTime asOnDate);
        Task<AccountLedgerDto> GetAccountLedgerAsync(int accountId, DateTime asOnDate);
    }
}
