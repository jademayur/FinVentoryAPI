namespace FinVentoryAPI.DTOs.FinancialReportDTOs
{
    public class BalanceResponseDto
    {
        public List<BalanceDto> Items { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
    }
}
