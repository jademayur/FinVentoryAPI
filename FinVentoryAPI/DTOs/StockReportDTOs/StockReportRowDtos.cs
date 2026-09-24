namespace FinVentoryAPI.DTOs.StockReportDTOs
{
    // ── 1. Stock Register ───────────────────────────────
    public class StockRegisterRowDto
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string GroupName { get; set; }
        public string WarehouseName { get; set; }
        public decimal Qty { get; set; }
        public string Unit { get; set; }
        public string Tracking { get; set; }
    }

    // ── 2. Stock Valuation ──────────────────────────────
    public class StockValuationRowDto
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string GroupName { get; set; }
        public string WarehouseName { get; set; }
        public decimal Qty { get; set; }
        public decimal AvgRate { get; set; }
        public decimal TotalValue { get; set; }
        public string Unit { get; set; }
    }

    // ── 3. Item Ledger ──────────────────────────────────
    public class ItemLedgerRowDto
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Date { get; set; }
        public string VoucherType { get; set; }
        public string VoucherNo { get; set; }
        public string PartyName { get; set; }
        public decimal InQty { get; set; }
        public decimal OutQty { get; set; }
        public decimal Balance { get; set; }
        public string WarehouseName { get; set; }
        public string Remarks { get; set; }
    }

    // ── 4. Stock Age Analysis ───────────────────────────
    public class StockAgeRowDto
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string GroupName { get; set; }
        public decimal Qty { get; set; }
        public string Unit { get; set; }
        public string LastPurchaseDate { get; set; }
        public int AgeDays { get; set; }
        public string AgeBucket { get; set; } // "0-30", "31-60", "61-90", "90+"
    }

    // ── 5. Dead Stock ───────────────────────────────────
    public class DeadStockRowDto
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string GroupName { get; set; }
        public decimal Qty { get; set; }
        public string Unit { get; set; }
        public string LastTransactionDate { get; set; }
        public int DaysInactive { get; set; }
        public decimal EstimatedValue { get; set; }
    }

    // ── 6. Stock Summary by Group ───────────────────────
    public class StockGroupSummaryRowDto
    {
        public int ItemGroupId { get; set; }
        public string GroupName { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalValue { get; set; }
    }

    // ── 7. Warehouse-wise Stock ─────────────────────────
    public class WarehouseStockRowDto
    {
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalValue { get; set; }
    }

    // ── Filter Options ──────────────────────────────────
    public class StockReportFilterOptionsDto
    {
        public List<StockReportIdNameDto> ItemGroups { get; set; } = new();
        public List<StockReportIdNameDto> Warehouses { get; set; } = new();
        public List<StockReportItemOptionDto> Items { get; set; } = new();
    }

    public class StockReportIdNameDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class StockReportItemOptionDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
    }
}
