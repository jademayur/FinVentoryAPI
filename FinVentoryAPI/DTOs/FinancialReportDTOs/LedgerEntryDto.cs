namespace FinVentoryAPI.DTOs.FinancialReportDTOs
{
    public class LedgerEntryDto
    {
        public DateTime? Date { get; set; }   // null = Opening Balance row
        public string Particulars { get; set; } = string.Empty;
        public string VoucherType { get; set; } = string.Empty;
        public string VoucherNo { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RunningBalance { get; set; }   // always positive magnitude
        public string BalanceType { get; set; } = string.Empty; // "Dr" | "Cr"
    }
}
