using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class ProductionIssueDetailSerial
    {
        [Key]
        public int Id { get; set; }
        public int ProductionIssueDetailId { get; set; }
        public int ItemSerialId { get; set; }

        [ForeignKey(nameof(ProductionIssueDetailId))]
        public ProductionIssueDetail? ProductionIssueDetail { get; set; }

        [ForeignKey(nameof(ItemSerialId))]
        public ItemSerial? Serial { get; set; }
    }
}
