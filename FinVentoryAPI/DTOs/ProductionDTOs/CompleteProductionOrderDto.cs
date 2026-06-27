namespace FinVentoryAPI.DTOs.ProductionDTOs
{
    public class CompleteProductionOrderDto
    {
        public decimal ActualQuantity { get; set; }         // finished good actual qty
        public DateOnly ActualCompletionDate { get; set; }
        public int? WarehouseId { get; set; }               // where to stock in/out
        public List<CompleteProductionOrderLineDto> Lines { get; set; } = new();
    }
}
