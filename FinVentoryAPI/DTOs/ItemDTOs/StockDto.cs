using FinVentoryAPI.DTOs.StockLedgerDTOs;

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

        public string ItemManageBy { get; set; }  // "None" | "Batch" | "Serial"
        public List<BatchStockDto> Batches { get; set; } = new();
        public List<SerialStockDto> Serials { get; set; } = new();
    }

}

