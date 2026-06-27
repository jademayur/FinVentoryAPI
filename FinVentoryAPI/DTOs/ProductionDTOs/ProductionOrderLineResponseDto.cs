namespace FinVentoryAPI.DTOs.ProductionDTOs
{
    public class ProductionOrderLineResponseDto
    {
        public int ProductionOrderLineId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }
        public int UnitId { get; set; }
        public decimal? WastagePercent { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }
    }
}
