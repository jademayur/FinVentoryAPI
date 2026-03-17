namespace FinVentoryAPI.Entities
{
    public class Warehouse : BaseEntity
    {
        public int  WarehouseId { get; set; }
        public int CompanyId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string? WarehouseCode { get; set; }
        public int? ParentWarehouseId { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? ContactPerson { get; set; }
        public string? MobileNo { get; set; }
        public virtual Warehouse? ParentWarehouse { get; set; }
    }
}
