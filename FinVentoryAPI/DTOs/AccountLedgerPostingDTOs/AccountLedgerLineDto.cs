namespace FinVentoryAPI.DTOs.AccountLedgerPostingDTOs
{
    public class AccountLedgerLineDto
    {
        public int AccountId { get; set; }
        public int BusinessPartnerId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Remarks { get; set; }
    }
}
