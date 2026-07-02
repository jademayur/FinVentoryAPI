using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class GRNTaxDetail
    {
        [Key]        
        public int TaxDetailId { get; set; }
        public int GRNId { get; set; }
        public int? GRNDetailId { get; set; }
        public int? TaxId { get; set; }

        public decimal IGSTRate { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal CessRate { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }

        // ── Navigation ───────────────────────────────────────────────────────
        [ForeignKey(nameof(GRNId))]
        public GRNMain? GRN { get; set; }

        [ForeignKey(nameof(GRNDetailId))]
        public GRNDetail? Detail { get; set; }

        [ForeignKey(nameof(TaxId))]
        public Tax? Tax { get; set; }
    }
}
