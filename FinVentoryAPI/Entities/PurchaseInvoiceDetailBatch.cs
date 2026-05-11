using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class PurchaseInvoiceDetailBatch
    {
        public int Id { get; set; }
        public int DetailId { get; set; }
        public int InvoiceId { get; set; }
        public int BatchId { get; set; }

        /// <summary>Qty received into this batch for this detail line.</summary>
        public decimal Qty { get; set; }

        [ForeignKey(nameof(DetailId))]
        public PurchaseInvoiceDetail? Detail { get; set; }

        [ForeignKey(nameof(BatchId))]
        public ItemBatch? Batch { get; set; }
    }
}
