namespace FinVentoryAPI.DTOs.ProductionDTOs
{
    public class UpdateProductionOrderDto
    {
       
        public int ItemId { get; set; }
        public int? BomId { get; set; }

        public decimal PlannedQuantity { get; set; }
        public int UnitId { get; set; }

        public string? Notes { get; set; }
        public string? RefNo { get; set; }
        public DateOnly? RefDate { get; set; }

        public DateOnly? PlannedStartDate { get; set; }
        public DateOnly? PlannedEndDate { get; set; }

        public List<CreateProductionOrderLineDto> Lines { get; set; } = new();
    }
}
