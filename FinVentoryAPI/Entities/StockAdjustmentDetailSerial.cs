using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class StockAdjustmentDetailSerial
    {
        [Key]
        public int Id { get; set; }
        public int AdjustmentDetailId { get; set; }
        public int ItemSerialId { get; set; }

        [ForeignKey(nameof(AdjustmentDetailId))]
        public StockAdjustmentDetail? Detail { get; set; }

        [ForeignKey(nameof(ItemSerialId))]
        public ItemSerial? Serial { get; set; }
    }
}
