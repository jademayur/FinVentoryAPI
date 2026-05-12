using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class OutgoingPaymentAllocation
    {
        [Key]
        public int AllocationId { get; set; }
        public int PaymentId { get; set; }
        public int BillId { get; set; }          // PurchaseInvoiceMain.BillId
        public decimal AmountApplied { get; set; }

        // ── Navigation ─────────────────────────────────────────────
        public OutgoingPaymentMain? Payment { get; set; }
        public PurchaseInvoiceMain? Bill { get; set; }
    }
}
