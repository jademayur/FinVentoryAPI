namespace FinVentoryAPI.DTOs.OpeningItemBalanceDTOs
{
    public class OpeningBatchResponseDto
    {
        public int BatchId { get; set; }
        public string BatchNo { get; set; } = string.Empty;
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Qty { get; set; }
        public decimal AvailableQty { get; set; }
    }
}
