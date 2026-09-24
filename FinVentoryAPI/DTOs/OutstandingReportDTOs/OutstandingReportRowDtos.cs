namespace FinVentoryAPI.DTOs.OutstandingReportDTOs
{
    public class OverallOutstandingRowDto
    {
        public int BusinessPartnerId { get; set; }
        public string PartyName { get; set; } = "";
        public string? GSTNo { get; set; }
        public int InvoiceCount { get; set; }
        public decimal TotalBilled { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalPending { get; set; }
        public int MaxOverdueDays { get; set; }
    }

    public class BillWiseOutstandingRowDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = "";
        public string? SupplierInvoiceNo { get; set; }
        public string InvoiceDate { get; set; } = "";
        public string DueDate { get; set; } = "";
        public string PartyName { get; set; } = "";
        public string? GSTNo { get; set; }
        public decimal InvoiceTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public int DaysOverdue { get; set; }
        public string AgingBucket { get; set; } = "";
    }

    public class AgingOutstandingRowDto
    {
        public string BucketName { get; set; } = "";
        public int BucketOrder { get; set; }
        public int InvoiceCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }
}
