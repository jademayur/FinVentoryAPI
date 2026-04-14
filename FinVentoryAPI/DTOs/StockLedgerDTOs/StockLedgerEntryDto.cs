namespace FinVentoryAPI.DTOs.StockLedgerDTOs
{
    public class StockLedgerEntryDto
    {
        public int LedgerId { get; set; }
        public DateTime Date { get; set; }
        public string VoucherType { get; set; } = string.Empty;
        public string VoucherNo { get; set; } = string.Empty;
        public string? PartyName { get; set; }
        public string? WarehouseName { get; set; }
        public decimal InQty { get; set; }   // always +ve
        public decimal OutQty { get; set; }   // always +ve
        public decimal Balance { get; set; }   // running balance
        public decimal? Rate { get; set; }
        public string? Remarks { get; set; }
    }
}
