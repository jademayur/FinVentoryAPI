using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class StockTransferDetailBatch
    {
        [Key]
        public int Id { get; set; }
        public int TransferDetailId { get; set; }
        public int ItemBatchId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Qty { get; set; }

        [ForeignKey(nameof(TransferDetailId))]
        public StockTransferDetail? Detail { get; set; }

        [ForeignKey(nameof(ItemBatchId))]
        public ItemBatch? Batch { get; set; }
    }
}
