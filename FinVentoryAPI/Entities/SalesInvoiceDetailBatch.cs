using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class SalesInvoiceDetailBatch
    {
        public int Id { get; set; }

        public int DetailId { get; set; }
        public int InvoiceId { get; set; }
        public int BatchId { get; set; }

        /// <summary>Qty taken from this batch for this detail line.</summary>
        public decimal Qty { get; set; }

        // ── Navigation ──────────────────────────────────────
        [ForeignKey(nameof(DetailId))]
        public SalesInvoiceDetail? Detail { get; set; }

        [ForeignKey(nameof(BatchId))]
        public ItemBatch? Batch { get; set; }
    }
}
