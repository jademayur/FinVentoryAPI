namespace FinVentoryAPI.DTOs.StockLedgerDTOs
{
    public class CreateStockLedgerDto
    {
        public int ItemId { get; set; }
        public int? WarehouseId { get; set; }
        public DateTime Date { get; set; }
        public string VoucherType { get; set; } = string.Empty;
        public string VoucherNo { get; set; } = string.Empty;
        public int? BusinessPartnerId { get; set; }
        public decimal Qty { get; set; }  // +ve = IN, -ve = OUT
        public decimal? Rate { get; set; }
        public string? Remarks { get; set; }
    }
}
