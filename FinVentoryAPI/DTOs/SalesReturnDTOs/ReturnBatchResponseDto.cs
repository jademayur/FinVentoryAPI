namespace FinVentoryAPI.DTOs.SalesReturnDTOs
{
    public class ReturnBatchResponseDto
    {
        public int Id { get; set; }
        public int DetailId { get; set; }
        public int BatchId { get; set; }
        public string? BatchNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Qty { get; set; }
    }
}
