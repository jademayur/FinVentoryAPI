namespace FinVentoryAPI.DTOs.SalesQuotationDTOs
{
    public class SalesQuotationPrintDto
    {
        // ── Company Details ──────────────────────────────
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyAddress { get; set; }
        public string? CompanyCity { get; set; }
        public string? CompanyState { get; set; }
        public string? CompanyPinCode { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyMobile { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyGstNumber { get; set; }
        public string? CompanyPanNumber { get; set; }

        // ── Quotation Header ─────────────────────────────
        public int QuotationId { get; set; }
        public string QuotationNo { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntilDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? RevisionNo { get; set; }

        // ── Customer Details ─────────────────────────────
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerCode { get; set; }
        public string? CustomerGstNumber { get; set; }
        public string? BillAddress { get; set; }
        public string? ShipAddress { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonMobile { get; set; }
        public string? SalesPersonName { get; set; }

        // ── GST Info ─────────────────────────────────────
        public string? SalesStateName { get; set; }
        public string? BillStateName { get; set; }
        public bool IsIntraState { get; set; }

        // ── Line Items ───────────────────────────────────
        public List<SalesQuotationPrintDetailDto> Details { get; set; } = new();

        // ── Tax Summary ──────────────────────────────────
        public List<SalesQuotationPrintTaxSummaryDto> TaxSummary { get; set; } = new();

        // ── Totals ───────────────────────────────────────
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetTotal { get; set; }
        public string? Remarks { get; set; }
    }

    public class SalesQuotationPrintDetailDto
    {
        public int SrNo { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public string? HsnCode { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }
        public decimal IgstRate { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal CgstRate { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstRate { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal LineTaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public bool IsTaxIncluded { get; set; }
    }

    public class SalesQuotationPrintTaxSummaryDto
    {
        public string TaxName { get; set; } = string.Empty;
        public decimal TaxableAmount { get; set; }
        public decimal IgstRate { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal CgstRate { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstRate { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
    }
}
