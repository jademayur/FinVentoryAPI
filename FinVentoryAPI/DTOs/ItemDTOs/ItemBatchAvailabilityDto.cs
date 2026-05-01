namespace FinVentoryAPI.DTOs.ItemDTOs
{
    public class ItemBatchAvailabilityDto
    {
        public int BatchId { get; set; }
        public string BatchNo { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public decimal AvailableQty { get; set; }
    }
}
