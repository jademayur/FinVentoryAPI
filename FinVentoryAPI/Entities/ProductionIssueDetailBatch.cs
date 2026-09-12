using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class ProductionIssueDetailBatch
    {
        [Key]
        public int Id { get; set; }
        public int ProductionIssueDetailId { get; set; }
        public int ItemBatchId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Qty { get; set; }

        [ForeignKey(nameof(ProductionIssueDetailId))]
        public ProductionIssueDetail? ProductionIssueDetail { get; set; }

        [ForeignKey(nameof(ItemBatchId))]
        public ItemBatch? Batch { get; set; }
    }
}
