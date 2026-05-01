using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class SalesInvoiceDetailSerial
    {
        public int Id { get; set; }
        public int DetailId { get; set; }
        public int InvoiceId { get; set; }
        public int SerialId { get; set; }

        // ── Navigation ──────────────────────────────────────
        [ForeignKey(nameof(DetailId))]
        public SalesInvoiceDetail? Detail { get; set; }

        [ForeignKey(nameof(SerialId))]
        public ItemSerial? Serial { get; set; }
    }
}
