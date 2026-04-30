namespace FinVentoryAPI.DTOs.OpeningItemBalanceDTOs
{
    public class OpeningBalanceMatItemResponseDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemManageBy { get; set; }   // "Regular" | "Batch" | "Serial"
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }

        public List<OpeningBatchResponseDto>? Batches { get; set; }
        public List<OpeningSerialResponseDto>? Serials { get; set; }
    }
}
