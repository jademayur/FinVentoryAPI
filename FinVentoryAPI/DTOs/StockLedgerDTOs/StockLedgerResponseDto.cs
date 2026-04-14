namespace FinVentoryAPI.DTOs.StockLedgerDTOs
{
    public class StockLedgerResponseDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? ItemGroupName { get; set; }
        public decimal OpeningStock { get; set; }
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal ClosingStock { get; set; }
        public List<StockLedgerEntryDto> Entries { get; set; } = new();
    }
}
