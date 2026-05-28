namespace FinVentoryAPI.DTOs.BomDTOs
{
    public class CreateBomLineDto
    {
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public decimal ConversionFactor { get; set; } = 1;
        public decimal WastagePercent { get; set; } = 0;
        public string? Notes { get; set; }
        public int SortOrder { get; set; } = 0;
    }
}
