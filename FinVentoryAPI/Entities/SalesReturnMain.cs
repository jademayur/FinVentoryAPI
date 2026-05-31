using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class SalesReturnMain : BaseEntity
    {
        [Key]
        public int ReturnId { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public string ReturnNo { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; }

        // Link back to original invoice (optional — user may enter manually)
        public int? OriginalInvoiceId { get; set; }
        public string? OriginalInvoiceNo { get; set; }
        public DateTime? OriginalInvoiceDate { get; set; }

        // "Credit" = credit note to customer | "Debit" = debit note raised by customer
        public string NoteType { get; set; } = "Credit";

        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }
        public int SalesAccountId { get; set; }

        public int? SalesStateCode { get; set; }
        public int? BillStateCode { get; set; }
        public int? BillAddressId { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetTotal { get; set; }

        // Export fields (for GSTR-1 Section 3.1)
        public bool? IsExport { get; set; }
        public bool? IsReverseCharge { get; set; }
        public bool? IsNonGST { get; set; }
        public string? PortCode { get; set; }
        public string? ShippingBillNo { get; set; }
        public DateTime? ShippingBillDate { get; set; }

        public string Status { get; set; } = "Draft";
        public string? Remarks { get; set; }       

        // Navigation
        public Company? Company { get; set; }
        public BusinessPartner? BusinessPartner { get; set; }
        public Location? Location { get; set; }
        public Account? SalesAccount { get; set; }
        public BusinessPartnerAddress? BillAddress { get; set; }
        public SalesInvoiceMain? OriginalInvoice { get; set; }

        public List<SalesReturnDetail>? Details { get; set; }
        public List<SalesReturnTaxDetail>? TaxDetails { get; set; }
    }
}
