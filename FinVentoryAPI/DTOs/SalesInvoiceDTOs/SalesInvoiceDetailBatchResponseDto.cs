namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class SalesInvoiceDetailBatchResponseDto
    {
        public int Id { get; set; }
        public int DetailId { get; set; }
        public int BatchId { get; set; }
        public string? BatchNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Qty { get; set; }
    }
}
