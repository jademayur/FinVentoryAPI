using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class ProductionReceiptDetail : BaseEntity
    {
        [Key]
        public int ProductionReceiptDetailId { get; set; }
        public int ProductionReceiptId { get; set; }
        public int ItemId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Qty { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [ForeignKey(nameof(ProductionReceiptId))]
        public ProductionReceiptMain? ProductionReceipt { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        public ICollection<ProductionReceiptDetailBatch> Batches { get; set; } = new List<ProductionReceiptDetailBatch>();
        public ICollection<ProductionReceiptDetailSerial> Serials { get; set; } = new List<ProductionReceiptDetailSerial>();
    }
}
