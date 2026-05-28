namespace FinVentoryAPI.DTOs.BomDTOs
{
    public class BomLineResponseDto
    {
        public int BomLineId { get; set; }
        public int BomId { get; set; }

        // Component item
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }

        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }

        public decimal ConversionFactor { get; set; }
        public decimal WastagePercent { get; set; }

        /// <summary>Quantity + wastage already factored in.</summary>
        public decimal EffectiveQuantity => Quantity * (1 + WastagePercent / 100);

        public string? Notes { get; set; }
        public int SortOrder { get; set; }
    }
}
