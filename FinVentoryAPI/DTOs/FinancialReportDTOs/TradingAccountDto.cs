namespace FinVentoryAPI.DTOs.FinancialReportDTOs
{
    public class TradingAccountDto
    {
        public List<BalanceGroupDto> IncomeGroups { get; set; } = new();
        public List<BalanceGroupDto> ExpenseGroups { get; set; } = new();
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal GrossProfit { get; set; }  // negative = Gross Loss
    }
}
