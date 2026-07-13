using FinVentoryAPI.DTOs.Dashboard;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<TodaySummaryDto> GetTodaySummaryAsync();
        Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync(int months);
        Task<List<OverdueReceivableDto>> GetOverdueReceivablesAsync();
        Task<List<OverduePayableDto>> GetOverduePayablesAsync();
        Task<List<CashBankBalanceDto>> GetCashBankBalancesAsync();
        Task<List<LowStockItemDto>> GetLowStockItemsAsync();
        Task<PendingDocsDto> GetPendingDocsAsync();
    }
}
