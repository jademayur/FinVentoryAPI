using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class StockAdjustmentMain : BaseEntity
    {
        [Key]
        public int AdjustmentId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }

        [MaxLength(30)]
        public string AdjustmentNo { get; set; } = string.Empty;

        public DateTime AdjustmentDate { get; set; }

        public int WarehouseId { get; set; }

        [MaxLength(20)]
        public string AdjustmentType { get; set; } = string.Empty; // Increase / Decrease

        [MaxLength(50)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        public int LocationId { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [ForeignKey(nameof(WarehouseId))]
        public Warehouse? Warehouse { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location? Location { get; set; }

        public ICollection<StockAdjustmentDetail> Details { get; set; } = new List<StockAdjustmentDetail>();
    }
}
