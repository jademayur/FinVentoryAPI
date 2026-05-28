using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class BomLine
    {
        [Key]
        public int BomLineId { get; set; }

        public int BomId { get; set; }

        // ── Component ──────────────────────────────────────────────
        public int ItemId { get; set; }           // Raw material / sub-assembly

        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        public int UnitId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal ConversionFactor { get; set; } = 1;

        /// <summary>Extra % added to Quantity to account for expected wastage.</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal WastagePercent { get; set; } = 0;

        [MaxLength(300)]
        public string? Notes { get; set; }

        public int SortOrder { get; set; } = 0;

        // ── Navigation ─────────────────────────────────────────────
        [ForeignKey("BomId")]
        public virtual BillOfMaterial? Bom { get; set; }

        [ForeignKey("ItemId")]
        public virtual Item? Component { get; set; }
    }
}
