namespace FinVentoryAPI.DTOs.AccountLedgerPostingDTOs
{
    public class AccountLedgerResponseDto
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountCode { get; set; } = string.Empty;
        public string? AccountGroupName { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<AccountLedgerEntryDto> Entries { get; set; } = new();
    }
}
