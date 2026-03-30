namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class SalesInvoiceTaxDetailResponseDto
    {
        public int TaxDetailId { get; set; }
        public int DetailId { get; set; }

        // Tax
        public int TaxId { get; set; }
        public string TaxName { get; set; } = string.Empty;
        public string? TaxType { get; set; }

        // Rates
        public decimal IGSTRate { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal CessRate { get; set; }

        // Amounts
        public decimal TaxableAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }

        // Posting Account Names (for display/audit)
        public string? IGSTPostingAccount { get; set; }
        public string? CGSTPostingAccount { get; set; }
        public string? SGSTPostingAccount { get; set; }
        public string? CessPostingAccount { get; set; }
    }
}
