using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class JobWorkReceiptDetail : BaseEntity
    {
        [Key]
        public int JobWorkReceiptDetailId { get; set; }
        public int JobWorkReceiptId { get; set; }
        public int ItemId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Qty { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [ForeignKey(nameof(JobWorkReceiptId))]
        public JobWorkReceiptMain? JobWorkReceipt { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        public ICollection<JobWorkReceiptDetailBatch> Batches { get; set; } = new List<JobWorkReceiptDetailBatch>();
        public ICollection<JobWorkReceiptDetailSerial> Serials { get; set; } = new List<JobWorkReceiptDetailSerial>();
    }
}
