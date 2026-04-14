namespace FinVentoryAPI.DTOs.StockLedgerDTOs
{
    public class StockLedgerLineDto
    {
        public int ItemId { get; set; }
        public decimal Qty { get; set; }  // caller decides sign
        public decimal? Rate { get; set; }
        public string? Remarks { get; set; }
    }
}
