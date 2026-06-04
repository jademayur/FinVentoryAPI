using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class SalesQuotationMain
    {
        [Key]
        public int QuotationId { get; set; }

        // ── Company / Year ────────────────────────────────────
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }

        // ── Identification ────────────────────────────────────
        [Required, MaxLength(50)]
        public string QuotationNo { get; set; } = string.Empty;   // e.g. QT-2526-0001

        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntilDate { get; set; }             // Expiry of the quote

        public int? ParentQuotationId { get; set; }
        public int RevisionNo { get; set; } = 0;

        // ── Business Partner ──────────────────────────────────
        public int BusinessPartnerId { get; set; }

        // ── Location ──────────────────────────────────────────
        public int LocationId { get; set; }

        // ── GST State Codes ───────────────────────────────────
        public int? SalesStateCode { get; set; }
        public int? BillStateCode { get; set; }

        // ── Optional References ───────────────────────────────
        public int? ContactPersonId { get; set; }
        public int? SalesPersonId { get; set; }
        public int? BillAddressId { get; set; }
        public int? ShipAddressId { get; set; }

        // ── Totals ────────────────────────────────────────────
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetTotal { get; set; }

        // ── Status ────────────────────────────────────────────
        // Values: Draft → Sent → Accepted / Rejected / Expired
        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        // ── Misc ──────────────────────────────────────────────
        [MaxLength(500)]
        public string? Remarks { get; set; }

        // ── Soft Delete / Audit ───────────────────────────────
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // ── Navigation Properties ─────────────────────────────
        public BusinessPartner? BusinessPartner { get; set; }
        public Location? Location { get; set; }

        [ForeignKey(nameof(ContactPersonId))]
        public BusinessPartnerContact? ContactPerson { get; set; }

        [ForeignKey(nameof(SalesPersonId))]
        public SalesPerson? SalesPerson { get; set; }

        [ForeignKey(nameof(BillAddressId))]
        public BusinessPartnerAddress? BillAddress { get; set; }

        [ForeignKey(nameof(ShipAddressId))]
        public BusinessPartnerAddress? ShipAddress { get; set; }

        public List<SalesQuotationDetail>? Details { get; set; }
        public List<SalesQuotationTaxDetail>? TaxDetails { get; set; }

        public SalesQuotationMain? ParentQuotation { get; set; }
        public ICollection<SalesQuotationMain>? Revisions { get; set; }
    }
}
