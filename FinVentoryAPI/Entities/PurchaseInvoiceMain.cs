using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class PurchaseInvoiceMain:BaseEntity
    {
        [Key]
        public int InvoiceId { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }

        // Invoice Info
        public string InvoiceNo { get; set; } = string.Empty;       // Auto-generated internal ref
        public string SupplierInvoiceNo { get; set; } = string.Empty; // Supplier's own invoice number
        public DateTime InvoiceDate { get; set; }
        public DateTime SupplierInvoiceDate { get; set; }
        public DateTime DueDate { get; set; }

        public int BusinessPartnerId { get; set; }   // Supplier
        public int LocationId { get; set; }          // Receiving warehouse/location

        // Payable account — from BusinessPartner.AccountId (fetched at journal posting time)
        // ✅ Only Purchase Book Account stored here
        public int PurchaseAccountId { get; set; }

        public decimal SubTotal { get; set; }        // Sum of all TaxableAmount
        public decimal TaxAmount { get; set; }       // Sum of IGST+CGST+SGST
        public decimal CessAmount { get; set; }      // Sum of all Cess
        public decimal RoundOff { get; set; }        // +/- rounding
        public decimal NetTotal { get; set; }        // SubTotal + TaxAmount + CessAmount + RoundOff

        public string? Remarks { get; set; }
        public string Status { get; set; } = "Draft";

        public int? PurchaseStateCode { get; set; }  // GstState enum int — supplier's state
        public int? BillStateCode { get; set; }      // GstState enum int — our company's state
        public int? ContactPersonId { get; set; }    // FK → BusinessPartnerContact.BPContactId
        public int? BillAddressId { get; set; }      // FK → BusinessPartnerAddress.BPAddressId (our billing)
        public int? ShipAddressId { get; set; }      // FK → BusinessPartnerAddress.BPAddressId (delivery)

        // Transport / Dispatch
        public string? TransportName { get; set; }
        public string? VehicleNo { get; set; }
        public string? LrNo { get; set; }
        public DateTime? LrDate { get; set; }

        // Navigation
        public BusinessPartner? BusinessPartner { get; set; }
        public Location? Location { get; set; }

        [ForeignKey(nameof(PurchaseAccountId))]
        public Account? PurchaseAccount { get; set; }

        public BusinessPartnerContact? ContactPerson { get; set; }
        public BusinessPartnerAddress? BillAddress { get; set; }
        public BusinessPartnerAddress? ShipAddress { get; set; }

        public ICollection<PurchaseInvoiceDetail>? Details { get; set; }
        public ICollection<PurchaseInvoiceTaxDetail>? TaxDetails { get; set; }

    }
}
