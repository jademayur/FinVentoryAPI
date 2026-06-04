namespace FinVentoryAPI.DTOs.SalesQuotationDTOs
{
    public class SalesQuotationResponseDto
    {
        public int QuotationId { get; set; }
        public int FinYearId { get; set; }
        public string QuotationNo { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntilDate { get; set; }
        public string Status { get; set; } = string.Empty;

        public int BusinessPartnerId { get; set; }
        public string BusinessPartnerName { get; set; } = string.Empty;
        public string BusinessPartnerCode { get; set; } = string.Empty;

        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;

        public int? SalesStateCode { get; set; }
        public string? SalesStateName { get; set; }

        public int? BillStateCode { get; set; }
        public string? BillStateName { get; set; }

        public int? ContactPersonId { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonMobile { get; set; }

        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }

        public int? BillAddressId { get; set; }
        public string? BillAddressLine { get; set; }

        public int? ShipAddressId { get; set; }
        public string? ShipAddressLine { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetTotal { get; set; }

        // Convenience aliases (for Angular grid / summary cards)
        public decimal Amount { get; set; }       // = SubTotal
        public decimal Discount { get; set; }     // sum of all discount amounts
        public decimal NetAmount { get; set; }    // = NetTotal

        public string? Remarks { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // ── Revision tracking ─────────────────────────────
        /// <summary>Null for root quotations. Points to the original QuotationId for revisions.</summary>
        public int? ParentQuotationId { get; set; }

        /// <summary>0 for original/copied quotations. 1, 2, 3... for revisions.</summary>
        public int RevisionNo { get; set; }

        public List<SalesQuotationDetailResponseDto> Details { get; set; } = new();
        public List<SalesQuotationTaxDetailResponseDto> TaxDetails { get; set; } = new();
    }
}
