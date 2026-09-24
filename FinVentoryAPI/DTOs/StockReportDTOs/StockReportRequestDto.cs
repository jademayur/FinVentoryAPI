namespace FinVentoryAPI.DTOs.StockReportDTOs
{
    public class StockReportRequestDto
    {
        public string ReportType { get; set; } = "StockRegister";
        // "StockRegister" | "StockValuation" | "ItemLedger"
        // "StockAge" | "DeadStock" | "StockGroupSummary" | "WarehouseStock"

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<int>? ItemGroupIds { get; set; }
        public List<int>? WarehouseIds { get; set; }
        public List<int>? ItemIds { get; set; }

        // Dead Stock: items with no movement in last N days
        public int DeadStockDays { get; set; } = 90;

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
