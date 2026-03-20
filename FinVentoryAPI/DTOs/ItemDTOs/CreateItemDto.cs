using FinVentoryAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.ItemDTOs
{
    public class CreateItemDto
    {

        [Required(ErrorMessage = "Item code is required.")]
        [MaxLength(50, ErrorMessage = "Item code cannot exceed 50 characters.")]
        public string ItemCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Item name is required.")]
        [MaxLength(200, ErrorMessage = "Item name cannot exceed 200 characters.")]
        public string ItemName { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Print name cannot exceed 200 characters.")]
        public string? PrintName { get; set; }

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Item type is required.")]
        public Enums.ItemType ItemType { get; set; }

        [Required(ErrorMessage = "Item category is required.")]
        public Enums.ItemCategory ItemCategory { get; set; }

        [MaxLength(100, ErrorMessage = "Barcode cannot exceed 100 characters.")]
        public string? Barcode { get; set; }

        // ── Classification ────────────────────────────────────────

        [Required(ErrorMessage = "Item group is required.")]
        public int ItemGroupId { get; set; }

        public int? BrandId { get; set; }

        [Required(ErrorMessage = "HSN code is required.")]
        public int HSNCodeId { get; set; }

        // ── Units & Packing ───────────────────────────────────────

        [Required(ErrorMessage = "Base unit is required.")]
        public int BaseUnitId { get; set; }

        public int? AlternateUnitId { get; set; }

        [Range(0.0001, double.MaxValue, ErrorMessage = "Conversion factor must be greater than 0.")]
        public decimal? ConversionFactor { get; set; }

        [MaxLength(200, ErrorMessage = "Packing description cannot exceed 200 characters.")]
        public string? PackingDescription { get; set; }

        // ── Inventory ─────────────────────────────────────────────

        public bool AllowNagativeStock { get; set; } = false;

        [Required(ErrorMessage = "Item manage by is required.")]
        public Enums.ItemManageBy ItemManageBy { get; set; }

        [Required(ErrorMessage = "Costing method is required.")]
        public Enums.CostingMethod CostingMethod { get; set; }

        // ── Accounting ────────────────────────────────────────────

        public int? InventoryAccountId { get; set; }
        public int? COGSAccountId { get; set; }
        public int? SalesAccountId { get; set; }
        public int? PurchaseAccountId { get; set; }

        public ICollection<ItemPrice>? Prices { get; set; }
    }
}
