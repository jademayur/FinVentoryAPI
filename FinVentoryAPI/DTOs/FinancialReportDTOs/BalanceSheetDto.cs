namespace FinVentoryAPI.DTOs.FinancialReportDTOs
{
    public class BalanceSheetDto
    {
        public List<BalanceGroupDto> AssetGroups { get; set; } = new();
        public List<BalanceGroupDto> LiabilityGroups { get; set; } = new();
        public decimal NetProfit { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
    }
}
