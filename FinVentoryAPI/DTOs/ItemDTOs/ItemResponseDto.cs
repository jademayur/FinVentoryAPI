using FinVentoryAPI.Enums;

namespace FinVentoryAPI.DTOs.ItemDTOs
{
    public class ItemResponseDto
    {
        public int ItemId { get; set; }
        public int CompanyId { get; set; }

        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string? PrintName { get; set; }
        public string? Description { get; set; }
        public string? Barcode { get; set; }

        public ItemType ItemType { get; set; }
        public ItemCategory ItemCategory { get; set; }

        public int ItemGroupId { get; set; }
        public string? ItemGroupName { get; set; }

        public int? BrandId { get; set; }
        public string? BrandName { get; set; }

        public int HSNCodeId { get; set; }
        public string? HSNCode { get; set; }

        public int BaseUnitId { get; set; }
        public string? BaseUnitName { get; set; }

        public int? AlternateUnitId { get; set; }
        public string? AlternateUnitName { get; set; }

        public decimal? ConversionFactor { get; set; }

        public bool AllowNagativeStock { get; set; }
        public ItemManageBy ItemManageBy { get; set; }
        public CostingMethod CostingMethod { get; set; }

        public int? InventoryAccountId { get; set; }
        public string? InventoryAccountName { get; set; }

        public int? COGSAccountId { get; set; }
        public string? COGSAccountName { get; set; }

        public int? SalesAccountId { get; set; }
        public string? SalesAccountName { get; set; }

        public int? PurchaseAccountId { get; set; }
        public string? PurchaseAccountName { get; set; }

        public List<ItemPriceResponseDto>? Prices { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}