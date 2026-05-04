namespace FinVentoryAPI.DTOs.StockLedgerDTOs
{
    public class SerialStockDto
    {
        public int SerialId { get; set; }
        public string SerialNo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;  
        public DateTime? PurchaseDate { get; set; }                 
        public DateTime? WarrantyExpiry { get; set; }
    }
}
