namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class SalesInvoiceDetailResponseDto
    {
        public int DetailId { get; set; }
        public int InvoiceId { get; set; }

        // Item
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }

        // HSN
        public int HsnId { get; set; }
        public string HsnCode { get; set; } = string.Empty;

        // Pricing
        public string PriceType { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AddisDiscountAmount { get; set; }
        public bool IsTaxIncluded { get; set; }

        // Amounts
        public decimal TaxableAmount { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }
        public decimal LineTaxAmount { get; set; }
        public decimal LineTotal { get; set; }

        // Tax lines for this detail
        public List<SalesInvoiceTaxDetailResponseDto> TaxDetails { get; set; } = new();
    }
}
