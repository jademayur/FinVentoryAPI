namespace FinVentoryAPI.DTOs.ProductionDTOs
{
    public class CreateProductionOrderDto
    {
        public DateOnly OrderDate { get; set; }

        public int ItemId { get; set; }             // finished good
        public int? BomId { get; set; }             // nullable — ad-hoc allowed

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
