namespace FinVentoryAPI.DTOs.FinancialReportDTOs
{
    public class AccountLedgerDto
    {
        public string AccountName { get; set; } = string.Empty;
        public List<LedgerEntryDto> Entries { get; set; } = new();
        public decimal ClosingDebit { get; set; }
        public decimal ClosingCredit { get; set; }
    }
}
