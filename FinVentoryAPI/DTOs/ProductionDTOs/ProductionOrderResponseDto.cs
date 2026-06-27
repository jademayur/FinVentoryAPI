namespace FinVentoryAPI.DTOs.ProductionDTOs
{
    public class ProductionOrderResponseDto
    {
        public int ProductionOrderId { get; set; }
        public string OrderNo { get; set; } = null!;
        public DateOnly OrderDate { get; set; }

        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }

        public int? BomId { get; set; }
        public string? BomName { get; set; }

        public decimal PlannedQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }
        public int UnitId { get; set; }

        public string Status { get; set; } = null!;         // "Draft", "InProgress" etc
        public int StatusId { get; set; }

        public string? Notes { get; set; }
        public string? RefNo { get; set; }
        public DateOnly? RefDate { get; set; }

        public DateOnly? PlannedStartDate { get; set; }
        public DateOnly? PlannedEndDate { get; set; }
        public DateOnly? ActualCompletionDate { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public List<ProductionOrderLineResponseDto> Lines { get; set; } = new();
    }
}
