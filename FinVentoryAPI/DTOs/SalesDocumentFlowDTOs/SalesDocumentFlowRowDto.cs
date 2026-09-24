namespace FinVentoryAPI.DTOs.SalesPipelineDTOs
{
    public class SalesDocumentFlowRowDto
    {
        // Quotation
        public int QuotationId { get; set; }
        public string QuotationNo { get; set; } = "";
        public DateTime QuotationDate { get; set; }
        public string QuotationStatus { get; set; } = "";
        public decimal QuotationAmount { get; set; }

        // Customer
        public int BusinessPartnerId { get; set; }
        public string CustomerName { get; set; } = "";

        // Order
        public int OrderCount { get; set; }
        public string? OrderNos { get; set; }
        public string? OrderStatuses { get; set; }
        public decimal OrderAmount { get; set; }

        // Delivery
        public int DeliveryCount { get; set; }
        public string? DeliveryNos { get; set; }
        public string? DeliveryStatuses { get; set; }
        public decimal DeliveryAmount { get; set; }

        // Invoice
        public int InvoiceCount { get; set; }
        public string? InvoiceNos { get; set; }
        public string? InvoiceStatuses { get; set; }
        public decimal InvoiceAmount { get; set; }

        // Payment
        public decimal PaymentAmount { get; set; }
        public decimal OutstandingAmount { get; set; }

        // Stage
        public string CurrentStage { get; set; } = "Quotation";
    }
}
