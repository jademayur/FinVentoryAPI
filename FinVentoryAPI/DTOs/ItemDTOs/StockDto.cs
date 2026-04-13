namespace FinVentoryAPI.DTOs.ItemDTOs
{
    public class StockDto
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int? ItemGroupId { get; set; }
        public string? ItemGroupName { get; set; }
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public decimal Stock { get; set; }
        public string? Unit { get; set; }

    }
}
