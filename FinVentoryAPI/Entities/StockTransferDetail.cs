using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class StockTransferDetail : BaseEntity
    {
        [Key]
        public int TransferDetailId { get; set; }
        public int TransferId { get; set; }
        public int ItemId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Qty { get; set; }

        [MaxLength(200)]
        public string? Remarks { get; set; }

        [ForeignKey(nameof(TransferId))]
        public StockTransferMain? Transfer { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        public ICollection<StockTransferDetailBatch> Batches { get; set; } = new List<StockTransferDetailBatch>();
        public ICollection<StockTransferDetailSerial> Serials { get; set; } = new List<StockTransferDetailSerial>();
    }
}
