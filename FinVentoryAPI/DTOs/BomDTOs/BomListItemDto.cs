namespace FinVentoryAPI.DTOs.BomDTOs
{
    public class BomListItemDto
    {
        public int BomId { get; set; }
        public string? BomCode { get; set; }
        public string BomName { get; set; } = string.Empty;
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public decimal OutputQuantity { get; set; }
        public string? BaseUnitName { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int LineCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
