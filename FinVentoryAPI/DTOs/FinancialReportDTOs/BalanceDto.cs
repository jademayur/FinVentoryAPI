namespace FinVentoryAPI.DTOs.FinancialReportDTOs
{
    public class BalanceDto
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
}
