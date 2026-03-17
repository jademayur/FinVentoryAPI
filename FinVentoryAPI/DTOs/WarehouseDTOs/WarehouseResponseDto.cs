namespace FinVentoryAPI.DTOs.WarehouseDTOs
{
    public class WarehouseResponseDto
    {
        public int WarehouseId { get; set; }          
        public string WarehouseName { get; set; } = string.Empty;
        public string? WarehouseCode { get; set; }
        public int? ParentWarehouseId { get; set; }
        public string? ParentWarehouseName { get; set; }
        public bool IsActive { get; set; }

    }
}
