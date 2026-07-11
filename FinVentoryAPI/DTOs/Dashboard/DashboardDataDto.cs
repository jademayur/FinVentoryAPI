namespace FinVentoryAPI.DTOs.Dashboard
{
    public class DashboardDataDto
    {
        public TodaySummaryDto TodaySummary { get; set; }
        public List<MonthlyTrendDto> MonthlyTrend { get; set; }
        public List<OverdueReceivableDto> OverdueReceivables { get; set; }
        public List<OverduePayableDto> OverduePayables { get; set; }
        public List<CashBankBalanceDto> CashBankBalances { get; set; }
        public List<LowStockItemDto> LowStockItems { get; set; }
        public PendingDocsDto PendingDocs { get; set; }
    }
}
