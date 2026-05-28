using FinVentoryAPI.Enums;

namespace FinVentoryAPI.DTOs.BomDTOs
{
    public class UpdateBomDto
    {
        public string? BomCode { get; set; }
        public string BomName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal OutputQuantity { get; set; } = 1;
        public BaseUnit BaseUnitId { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public List<CreateBomLineDto> Lines { get; set; } = new();
    }
}
