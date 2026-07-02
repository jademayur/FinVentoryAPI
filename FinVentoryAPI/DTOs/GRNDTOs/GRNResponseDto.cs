namespace FinVentoryAPI.DTOs.GRNDTOs
{
    public class GRNResponseDto
    {
        public int GRNId { get; set; }
        public int FinYearId { get; set; }
        public string GRNNo { get; set; } = string.Empty;
        public DateTime GRNDate { get; set; }
        public string Status { get; set; } = string.Empty;

        public string? SupplierInvoiceNo { get; set; }
        public DateTime? SupplierInvoiceDate { get; set; }
        public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }
        public string? Remarks { get; set; }

        // Supplier
        public int BusinessPartnerId { get; set; }
        public string BusinessPartnerName { get; set; } = string.Empty;
        public string BusinessPartnerCode { get; set; } = string.Empty;

        // Location
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;

        // States
        public int? PurchaseStateCode { get; set; }
        public string? PurchaseStateName { get; set; }
        public int? BillStateCode { get; set; }
        public string? BillStateName { get; set; }

        // Contact / Purchase person
        public int? ContactPersonId { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonMobile { get; set; }
      
        // Addresses
        public int? BillAddressId { get; set; }
        public string? BillAddressLine { get; set; }
        public int? ShipAddressId { get; set; }
        public string? ShipAddressLine { get; set; }

        // Totals
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetTotal { get; set; }
        public decimal Discount { get; set; }

        // Audit
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public List<GRNDetailResponseDto> Details { get; set; } = new();
        public List<GRNTaxDetailResponseDto> TaxDetails { get; set; } = new();
    }
}
