namespace FinVentoryAPI.DTOs.Dashboard
{
    public class OverdueReceivableDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public int OverdueDays { get; set; }
        public string BusinessPartnerName { get; set; }
        public decimal NetTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
    }
}
