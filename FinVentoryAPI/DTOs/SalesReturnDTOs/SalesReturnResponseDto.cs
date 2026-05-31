namespace FinVentoryAPI.DTOs.SalesReturnDTOs
{
    public class SalesReturnResponseDto
    {
        public int ReturnId { get; set; }
        public int FinYearId { get; set; }
        public string ReturnNo { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; }
        public int? OriginalInvoiceId { get; set; }
        public string? OriginalInvoiceNo { get; set; }
        public DateTime? OriginalInvoiceDate { get; set; }
        public string NoteType { get; set; } = "Credit";
        public int BusinessPartnerId { get; set; }
        public string BusinessPartnerName { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int SalesAccountId { get; set; }
        public string SalesAccountName { get; set; } = string.Empty;
        public int? SalesStateCode { get; set; }
        public int? BillStateCode { get; set; }
        public int? BillAddressId { get; set; }
        public string? BillAddressLine { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetTotal { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<SalesReturnDetailResponseDto> Details { get; set; } = new();
        public List<SalesReturnTaxDetailResponseDto> TaxDetails { get; set; } = new();
    }
}
