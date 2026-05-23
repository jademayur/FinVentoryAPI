namespace FinVentoryAPI.DTOs.FinancialReportDTOs
{
    public class ProfitAndLossDto
    {
        public decimal GrossProfit { get; set; }
        public List<BalanceGroupDto> IncomeGroups { get; set; } = new();
        public List<BalanceGroupDto> ExpenseGroups { get; set; } = new();
        public decimal IndirectIncome { get; set; }
        public decimal IndirectExpense { get; set; }
        public decimal NetProfit { get; set; }  // negative = Net Loss
    }
}
