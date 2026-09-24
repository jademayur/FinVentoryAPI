using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class StockAdjustmentDetail : BaseEntity
    {
        [Key]
        public int AdjustmentDetailId { get; set; }
        public int AdjustmentId { get; set; }
        public int ItemId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Qty { get; set; }

        [MaxLength(200)]
        public string? Remarks { get; set; }

        [ForeignKey(nameof(AdjustmentId))]
        public StockAdjustmentMain? Adjustment { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        public ICollection<StockAdjustmentDetailBatch> Batches { get; set; } = new List<StockAdjustmentDetailBatch>();
        public ICollection<StockAdjustmentDetailSerial> Serials { get; set; } = new List<StockAdjustmentDetailSerial>();
    }
}
