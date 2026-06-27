namespace FinVentoryAPI.DTOs.ProductionDTOs
{
    public class ProductionOrderListItemDto
    {
        public int ProductionOrderId { get; set; }
        public string OrderNo { get; set; } = null!;
        public DateOnly OrderDate { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public string? BomName { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }
        public string Status { get; set; } = null!;
        public int StatusId { get; set; }
        public int LineCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
