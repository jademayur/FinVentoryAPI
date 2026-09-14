using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class ProductionReceiptDetailSerial
    {
        [Key]
        public int Id { get; set; }
        public int ProductionReceiptDetailId { get; set; }
        public int ItemSerialId { get; set; }

        [ForeignKey(nameof(ProductionReceiptDetailId))]
        public ProductionReceiptDetail? ProductionReceiptDetail { get; set; }

        [ForeignKey(nameof(ItemSerialId))]
        public ItemSerial? Serial { get; set; }
    }
}
