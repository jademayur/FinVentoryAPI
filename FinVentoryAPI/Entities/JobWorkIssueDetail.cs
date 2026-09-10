using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class JobWorkIssueDetail : BaseEntity
    {
        [Key]
        public int JobWorkIssueDetailId { get; set; }
        public int JobWorkIssueId { get; set; }
        public int ItemId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Qty { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [ForeignKey(nameof(JobWorkIssueId))]
        public JobWorkIssueMain? JobWorkIssue { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        public ICollection<JobWorkIssueDetailBatch> Batches { get; set; } = new List<JobWorkIssueDetailBatch>();
        public ICollection<JobWorkIssueDetailSerial> Serials { get; set; } = new List<JobWorkIssueDetailSerial>();
    }
}
