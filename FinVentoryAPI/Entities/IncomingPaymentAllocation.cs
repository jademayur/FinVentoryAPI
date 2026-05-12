using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class IncomingPaymentAllocation
    {
        [Key]
        public int AllocationId { get; set; }
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public decimal AmountApplied { get; set; }

        // ── Navigation ─────────────────────────────────────────────
        public IncomingPaymentMain? Payment { get; set; }
        public SalesInvoiceMain? Invoice { get; set; }
    }
}
