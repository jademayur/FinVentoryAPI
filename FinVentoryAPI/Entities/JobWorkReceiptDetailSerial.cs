using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class JobWorkReceiptDetailSerial
    {
        [Key]
        public int Id { get; set; }
        public int JobWorkReceiptDetailId { get; set; }
        public int ItemSerialId { get; set; }

        [ForeignKey(nameof(JobWorkReceiptDetailId))]
        public JobWorkReceiptDetail? JobWorkReceiptDetail { get; set; }

        [ForeignKey(nameof(ItemSerialId))]
        public ItemSerial? Serial { get; set; }
    }
}
