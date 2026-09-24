namespace FinVentoryAPI.DTOs.PurchaseDocumentFlowDTOs
{
    public class PurchaseDocumentFlowRowDto
    {
        public int OrderId { get; set; }
        public string OrderNo { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; } = "";
        public decimal OrderAmount { get; set; }

        public int BusinessPartnerId { get; set; }
        public string SupplierName { get; set; } = "";

        // GRN
        public int GrnCount { get; set; }
        public string? GrnNos { get; set; }
        public string? GrnStatuses { get; set; }
        public decimal GrnAmount { get; set; }

        // Invoice
        public int InvoiceCount { get; set; }
        public string? InvoiceNos { get; set; }
        public string? InvoiceStatuses { get; set; }
        public decimal InvoiceAmount { get; set; }

        // Payment
        public decimal PaymentAmount { get; set; }

        // Outstanding
        public decimal OutstandingAmount { get; set; }

        // Stage
        public string CurrentStage { get; set; } = "";
    }
}
