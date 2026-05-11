using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class PurchaseInvoiceDetailSerial
    {
        public int Id { get; set; }
        public int DetailId { get; set; }
        public int InvoiceId { get; set; }
        public int SerialId { get; set; }

        [ForeignKey(nameof(DetailId))]
        public PurchaseInvoiceDetail? Detail { get; set; }

        [ForeignKey(nameof(SerialId))]
        public ItemSerial? Serial { get; set; }
    }
}
