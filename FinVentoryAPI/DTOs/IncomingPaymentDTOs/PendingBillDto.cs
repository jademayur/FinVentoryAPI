namespace FinVentoryAPI.DTOs.IncomingPaymentDTOs
{
    public class PendingBillDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal InvoiceTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public int DaysOverdue { get; set; }
    }
}
