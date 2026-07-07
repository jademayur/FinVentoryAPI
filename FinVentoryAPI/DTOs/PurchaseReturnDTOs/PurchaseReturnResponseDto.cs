namespace FinVentoryAPI.DTOs.PurchaseReturnDTOs
{
    public class PurchaseReturnResponseDto
    {
        public int ReturnId { get; set; }
        public int FinYearId { get; set; }
        public string ReturnNo { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; }
        public int? OriginalInvoiceId { get; set; }
        public string? OriginalInvoiceNo { get; set; }
        public DateTime? OriginalInvoiceDate { get; set; }
        public string NoteType { get; set; } = "Debit";
        public int BusinessPartnerId { get; set; }
        public string BusinessPartnerName { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int PurchaseAccountId { get; set; }
        public string PurchaseAccountName { get; set; } = string.Empty;
        public int? PurchaseStateCode { get; set; }
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
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<PurchaseReturnDetailResponseDto> Details { get; set; } = new();
        public List<PurchaseReturnTaxDetailResponseDto> TaxDetails { get; set; } = new();
    }
}
