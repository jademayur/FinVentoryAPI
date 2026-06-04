using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class SalesQuotationTaxDetail
    {
        [Key]
        public int TaxDetailId { get; set; }

        public int QuotationId { get; set; }   // FK → SalesQuotationMain
        public int DetailId { get; set; }      // FK → SalesQuotationDetail
        public int TaxId { get; set; }         // FK → Tax

        // ── Tax Rates — copied from Tax entity at time of save ─
        public decimal IGSTRate { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal SGSTRate { get; set; }

        // ── Tax Amounts ───────────────────────────────────────
        public decimal TaxableAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }

       
        // ── Cess — copied from Hsn entity at time of save ─────
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }
               // ── Total ─────────────────────────────────────────────
        public decimal TotalTaxAmount { get; set; }      // IGST+CGST+SGST+Cess

        // ── Navigation Properties ─────────────────────────────
        public SalesQuotationMain? Quotation { get; set; }
        public SalesQuotationDetail? Detail { get; set; }
        public Tax? Tax { get; set; }


    }
}
