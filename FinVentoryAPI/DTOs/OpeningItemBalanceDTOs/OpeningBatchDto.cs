namespace FinVentoryAPI.DTOs.OpeningItemBalanceDTOs
{
    public class OpeningBatchDto
    {
        public int? BatchId { get; set; }
        public string? BatchNo { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Qty { get; set; }
    }
}
