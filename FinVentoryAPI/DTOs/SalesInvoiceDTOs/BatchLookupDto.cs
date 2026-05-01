namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class BatchLookupDto
    {
        public int BatchId { get; set; }
        public string BatchNo { get; set; } = string.Empty;
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal AvailableQty { get; set; }
    }
}
