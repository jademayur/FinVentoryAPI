namespace FinVentoryAPI.DTOs.StockLedgerDTOs
{
    public class BatchStockDto
    {
        public int BatchId { get; set; }
        public string BatchNo { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal AvailableQty { get; set; }
    }
}
