namespace FinVentoryAPI.DTOs.ProductionDTOs
{
    public class CreateProductionOrderLineDto
    {
        public int ItemId { get; set; }             // raw material
        public decimal PlannedQuantity { get; set; }
        public int UnitId { get; set; }
        public decimal? WastagePercent { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }
    }
}
