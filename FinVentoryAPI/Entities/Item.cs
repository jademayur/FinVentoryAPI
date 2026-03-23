using FinVentoryAPI.Enums;

namespace FinVentoryAPI.Entities
{
    public class Item : BaseEntity
    {
        public int ItemId { get; set; }
        public int CompanyId { get; set; }

        // Basic Info
        public string? ItemCode { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ItemType ItemType { get; set; } 
        // Goods / Service
        public ItemCategory ItemCategory { get; set; } 
        // RawMaterial / SemiFinished / FinishedGoods / Trading
        public string? Barcode { get; set; }

        // Classification
        public int ItemGroupId { get; set; }
        public int? BrandId { get; set; }
        public int HSNCodeId { get; set; }

        // Units & Packing
        public int BaseUnitId { get; set; }
        public int? AlternateUnitId { get; set; }
        public decimal? ConversionFactor { get; set; } // e.g. 1 Box = 10 Nos
        // Inventory
        public bool AllowNagativeStock { get; set; } = false;
        public ItemManageBy ItemManageBy { get; set; } 
        // Regular / Batch / Serial

        public CostingMethod CostingMethod { get; set; }
        // MovingAverage / FIFO / Standard

        // 🔗 Accounting
        public int? InventoryAccountId { get; set; }
        public int? COGSAccountId { get; set; }
        public int? SalesAccountId { get; set; }
        public int? PurchaseAccountId { get; set; }


        // 🔗 Navigation
        public Account? InventoryAccount { get; set; }
        public Account? COGSAccount { get; set; }
        public Account? SalesAccount { get; set; }
        public Account? PurchaseAccount { get; set; }

        public ICollection<ItemPrice>? Prices { get; set; }
        public ItemGroup? ItemGroup { get; set; }
        public Brand? Brand { get; set; }
        public Hsn? Hsn { get; set; }
    }
}
