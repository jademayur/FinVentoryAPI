using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class SalesQuotationDetail
    {
        [Key]
        public int DetailId { get; set; }

        // ── FK to Main ────────────────────────────────────────
        public int QuotationId { get; set; }

        // ── Item / HSN ────────────────────────────────────────
        public int ItemId { get; set; }
        public int HsnId { get; set; }

        [MaxLength(50)]
        public string? HsnCode { get; set; }             // Snapshot of HsnName at save time

        // ── Pricing ───────────────────────────────────────────
        [MaxLength(20)]
        public string? PriceType { get; set; }           // e.g. Retail / Wholesale

        public decimal Qty { get; set; }
        public decimal Rate { get; set; }

        // ── Discounts ─────────────────────────────────────────
        public decimal DiscountRate { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public decimal AddisDiscountAmount { get; set; }

        // ── Tax Calculation ───────────────────────────────────
        public bool IsTaxIncluded { get; set; }
        public decimal TaxableAmount { get; set; }

        // ── Cess ──────────────────────────────────────────────
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }

        // ── Line Totals ───────────────────────────────────────
        public decimal LineTaxAmount { get; set; }       // IGST+CGST+SGST+Cess
        public decimal LineTotal { get; set; }           // TaxableAmount + LineTaxAmount

        // ── Navigation Properties ─────────────────────────────
        public SalesQuotationMain? Quotation { get; set; }
        public Item? Item { get; set; }
        public Hsn? Hsn { get; set; }
        public List<SalesQuotationTaxDetail>? TaxDetails { get; set; }
    }
}
