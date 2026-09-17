using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class StockTransferDetailSerial
    {
        [Key]
        public int Id { get; set; }
        public int TransferDetailId { get; set; }
        public int ItemSerialId { get; set; }

        [ForeignKey(nameof(TransferDetailId))]
        public StockTransferDetail? Detail { get; set; }

        [ForeignKey(nameof(ItemSerialId))]
        public ItemSerial? Serial { get; set; }
    }
}
