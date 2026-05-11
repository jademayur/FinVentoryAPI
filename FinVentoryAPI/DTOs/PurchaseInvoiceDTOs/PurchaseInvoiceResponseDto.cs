namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class PurchaseInvoiceResponseDto
    {
        public int InvoiceId { get; set; }
        public int FinYearId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public string SupplierInvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime SupplierInvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;

        public int BusinessPartnerId { get; set; }
        public string BusinessPartnerName { get; set; } = string.Empty;
        public string BusinessPartnerCode { get; set; } = string.Empty;

        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;

        public int PurchaseAccountId { get; set; }
        public string PurchaseAccountName { get; set; } = string.Empty;
        public int PayableAccountId { get; set; }

        public int? PurchaseStateCode { get; set; }
        public string? PurchaseStateName { get; set; }

        public int? BillStateCode { get; set; }
        public string? BillStateName { get; set; }

        public int? ContactPersonId { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonMobile { get; set; }

        public int? BillAddressId { get; set; }
        public string? BillAddressLine { get; set; }

        public int? ShipAddressId { get; set; }
        public string? ShipAddressLine { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetTotal { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }

        public string? Remarks { get; set; }
        public string? TransportName { get; set; }
        public string? VehicleNo { get; set; }
        public string? LrNo { get; set; }
        public DateTime? LrDate { get; set; }

        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public List<PurchaseInvoiceDetailResponseDto> Details { get; set; } = new();
        public List<PurchaseInvoiceTaxDetailResponseDto> TaxDetails { get; set; } = new();
    }
}
