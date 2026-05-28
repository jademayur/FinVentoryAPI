using FinVentoryAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class BillOfMaterial : BaseEntity
    {
        [Key]
        public int BomId { get; set; }

        // ── Tenant ─────────────────────────────────────────────────
        public int CompanyId { get; set; }

        // ── Finished Good ──────────────────────────────────────────
        public int ItemId { get; set; }           // The item this BOM produces

        [MaxLength(50)]
        public string? BomCode { get; set; }

        [Required, MaxLength(200)]
        public string BomName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        // ── Output ─────────────────────────────────────────────────
        [Column(TypeName = "decimal(18,4)")]
        public decimal OutputQuantity { get; set; } = 1;  // How many units this BOM produces

        public BaseUnit BaseUnitId { get; set; }               // Unit of output quantity

        // ── Flags ──────────────────────────────────────────────────
        public bool IsDefault { get; set; } = false;      // Default BOM for this item
       
               

        // ── Navigation ─────────────────────────────────────────────
        [ForeignKey("ItemId")]
        public virtual Item? FinishedGood { get; set; }

        public virtual ICollection<BomLine> Lines { get; set; } = new List<BomLine>();
    }
}
