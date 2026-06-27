namespace FinVentoryAPI.Entities
{
    public class ProductionOrderLine
    {
        public int ProductionOrderLineId { get; set; }
        public int ProductionOrderId { get; set; }
        public ProductionOrder ProductionOrder { get; set; } = null!;

        public int ItemId { get; set; }                     // raw material / component
        public Item Component { get; set; } = null!;

        public decimal PlannedQuantity { get; set; }        // prefilled from BOM
        public decimal? ActualQuantity { get; set; }        // entered on completion

        public int UnitId { get; set; }
        public decimal? WastagePercent { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }
    }
}
