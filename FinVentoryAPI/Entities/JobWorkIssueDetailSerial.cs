using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class JobWorkIssueDetailSerial
    {
        [Key]
        public int Id { get; set; }
        public int JobWorkIssueDetailId { get; set; }
        public int ItemSerialId { get; set; }

        [ForeignKey(nameof(JobWorkIssueDetailId))]
        public JobWorkIssueDetail? JobWorkIssueDetail { get; set; }

        [ForeignKey(nameof(ItemSerialId))]
        public ItemSerial? Serial { get; set; }
    }
}
