using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class StockAdjustmentDetailBatch
    {
        [Key]
        public int Id { get; set; }
        public int AdjustmentDetailId { get; set; }
        public int ItemBatchId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Qty { get; set; }

        [ForeignKey(nameof(AdjustmentDetailId))]
        public StockAdjustmentDetail? Detail { get; set; }

        [ForeignKey(nameof(ItemBatchId))]
        public ItemBatch? Batch { get; set; }
    }
}
