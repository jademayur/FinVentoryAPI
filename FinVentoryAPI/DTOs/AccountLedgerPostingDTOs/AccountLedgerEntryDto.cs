namespace FinVentoryAPI.DTOs.AccountLedgerPostingDTOs
{
    public class AccountLedgerEntryDto
    {
        public int PostingId { get; set; }
        public DateTime Date { get; set; }
        public string VoucherType { get; set; } = string.Empty;
        public string VoucherNo { get; set; } = string.Empty;
        public string? PartyName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }   // running balance (Debit - Credit)
        public string? Remarks { get; set; }
    }
}
