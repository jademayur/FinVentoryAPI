namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class SalesInvoiceResponseDto
    {
        public int InvoiceId { get; set; }
        public int FinYearId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;

        // Business Partner
        public int BusinessPartnerId { get; set; }
        public string BusinessPartnerName { get; set; } = string.Empty;
        public string BusinessPartnerCode { get; set; } = string.Empty;

        // Location
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;

        // Account
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;

        // Totals
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetTotal { get; set; }

        public string? Remarks { get; set; }

        // Lines
        public List<SalesInvoiceDetailResponseDto> Details { get; set; } = new();
        public List<SalesInvoiceTaxDetailResponseDto> TaxDetails { get; set; } = new();

        // Audit
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
