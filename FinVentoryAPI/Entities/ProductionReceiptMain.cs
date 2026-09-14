using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class ProductionReceiptMain : BaseEntity
    {
        [Key]
        public int ProductionReceiptId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }

        [MaxLength(30)]
        public string ReceiptNo { get; set; } = string.Empty;

        public DateTime ReceiptDate { get; set; }
        public int? ProductionOrderId { get; set; }
        public int WarehouseId { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        public int LocationId { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [ForeignKey(nameof(WarehouseId))]
        public Warehouse? Warehouse { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location? Location { get; set; }

        [ForeignKey(nameof(ProductionOrderId))]
        public ProductionOrder? ProductionOrder { get; set; }

        public ICollection<ProductionReceiptDetail> Details { get; set; } = new List<ProductionReceiptDetail>();
    }
}
