using FinVentoryAPI.Enums;

namespace FinVentoryAPI.DTOs.BomDTOs
{
    public class BomResponseDto
    {
        public int BomId { get; set; }
        public int CompanyId { get; set; }

        // Finished good
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }

        public string? BomCode { get; set; }
        public string BomName { get; set; } = string.Empty;
        public string? Description { get; set; }

        public decimal OutputQuantity { get; set; }
        public int BaseUnitId { get; set; }
        public string? BaseUnitName { get; set; }

        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public List<BomLineResponseDto> Lines { get; set; } = new();
    }
}
