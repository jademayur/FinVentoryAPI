namespace FinVentoryAPI.DTOs.FinancialReportDTOs
{
    public class BalanceGroupDto
    {
        public string GroupName { get; set; }
        public List<BalanceDto> Items { get; set; }
    }
}
