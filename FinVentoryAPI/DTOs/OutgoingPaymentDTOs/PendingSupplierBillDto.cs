namespace FinVentoryAPI.DTOs.OutgoingPaymentDTOs
{
    public class PendingSupplierBillDto
    {
        public int BillId { get; set; }           // = PurchaseInvoiceMain.InvoiceId (int PK)
        public string BillNo { get; set; } = string.Empty;  // = PurchaseInvoiceMain.InvoiceNo
        public string SupplierInvoiceNo { get; set; } = string.Empty; // supplier's own ref
        public DateTime BillDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal BillTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public int DaysOverdue { get; set; }
    }
}
