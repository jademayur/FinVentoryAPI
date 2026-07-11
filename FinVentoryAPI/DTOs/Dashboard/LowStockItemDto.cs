namespace FinVentoryAPI.DTOs.Dashboard
{
    public class LowStockItemDto
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal AvailableQty { get; set; }
        public decimal ReorderLevel { get; set; }
        public string Unit { get; set; }

    }
}
