using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class GRNMain: BaseEntity
    {
        [Key]       
        public int GRNId { get; set; }

        public int CompanyId { get; set; }
        public int FinYearId { get; set; }

        [Required, MaxLength(30)]
        public string GRNNo { get; set; } = string.Empty;

        public DateTime GRNDate { get; set; }

        // Supplier reference: supplier's invoice / delivery challan
        [MaxLength(50)]
        public string? SupplierInvoiceNo { get; set; }
        public DateTime? SupplierInvoiceDate { get; set; }

        [MaxLength(50)]
        public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        // Status: Draft | Confirmed | Cancelled
        [Required, MaxLength(20)]
        public string Status { get; set; } = "Draft";

        // ── Parties ─────────────────────────────────────────────────────────
        public int BusinessPartnerId { get; set; }          // Supplier
        public int LocationId { get; set; }                 // Receiving warehouse
        public int? ContactPersonId { get; set; }
       
        // ── Addresses ────────────────────────────────────────────────────────
        public int? BillAddressId { get; set; }             // Our billing address
        public int? ShipAddressId { get; set; }             // Delivery / receiving address

        // ── GST state codes ──────────────────────────────────────────────────
        public int? PurchaseStateCode { get; set; }         // Our GSTIN state
        public int? BillStateCode { get; set; }             // Supplier GSTIN state

        // ── Financials ───────────────────────────────────────────────────────
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetTotal { get; set; }

      

        // ── Navigation properties ────────────────────────────────────────────
        [ForeignKey(nameof(BusinessPartnerId))]
        public BusinessPartner? BusinessPartner { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location? Location { get; set; }

        [ForeignKey(nameof(ContactPersonId))]
        public BusinessPartnerContact? ContactPerson { get; set; }
               

        [ForeignKey(nameof(BillAddressId))]
        public BusinessPartnerAddress? BillAddress { get; set; }

        [ForeignKey(nameof(ShipAddressId))]
        public BusinessPartnerAddress? ShipAddress { get; set; }

        public List<GRNDetail>? Details { get; set; }
        public List<GRNTaxDetail>? TaxDetails { get; set; }
    }
}
