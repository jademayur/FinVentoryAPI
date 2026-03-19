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
        public string ItemType { get; set; } = string.Empty;
        // Goods / Service
        public string ItemCategory { get; set; } = string.Empty;
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
        public bool AllowNagativeStock { get; set; }
        public string ItemManageBy { get; set; } 
        // Regular / Batch / Serial

        public string CostingMethod { get; set; }
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
       
    }
}
