namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class CreateItemBatchDto
    {
        public int ItemId { get; set; }
        public string BatchNo { get; set; } = string.Empty;
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal ReceivedQty { get; set; }
        public string? Remarks { get; set; }
    }
}
