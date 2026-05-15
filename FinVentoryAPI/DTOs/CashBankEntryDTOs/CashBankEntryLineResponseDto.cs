namespace FinVentoryAPI.DTOs.CashBankEntryDTOs
{
    public class CashBankEntryLineResponseDto
    {
        public int CashBankEntryLineId { get; set; }
        public int CashBankEntryId { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; } = null!;
        public string? AccountCode { get; set; }
        public string AccountType { get; set; } = null!;

        public string DrCr { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Narration { get; set; }
        public int SortOrder { get; set; }
    }
}
