namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class PurchaseInvoiceDetailResponseDto
    {
        public int DetailId { get; set; }
        public int InvoiceId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public int HsnId { get; set; }
        public string HsnCode { get; set; } = string.Empty;
        public string PriceType { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AddisDiscountAmount { get; set; }
        public bool IsTaxIncluded { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }
        public decimal LineTaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public string? ItemManageBy { get; set; }

        // Response uses BatchId/SerialId (these exist after creation)
        public List<PurchaseInvoiceDetailBatchResponseDto>? Batches { get; set; }
        public List<PurchaseInvoiceDetailSerialResponseDto>? Serials { get; set; }
        public List<PurchaseInvoiceTaxDetailResponseDto>? TaxDetails { get; set; }
    }
}
