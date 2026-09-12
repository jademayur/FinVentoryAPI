using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class ProductionIssueDetail : BaseEntity
    {
        [Key]
        public int ProductionIssueDetailId { get; set; }
        public int ProductionIssueId { get; set; }
        public int ItemId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Qty { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [ForeignKey(nameof(ProductionIssueId))]
        public ProductionIssueMain? ProductionIssue { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        public ICollection<ProductionIssueDetailBatch> Batches { get; set; } = new List<ProductionIssueDetailBatch>();
        public ICollection<ProductionIssueDetailSerial> Serials { get; set; } = new List<ProductionIssueDetailSerial>();
    }
}
