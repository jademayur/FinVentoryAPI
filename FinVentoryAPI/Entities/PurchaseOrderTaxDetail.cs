using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class PurchaseOrderTaxDetail
    {
        [Key]       
        public int TaxDetailId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public PurchaseOrderMain? Order { get; set; }

        [Required]
        public int OrderDetailId { get; set; }

        [ForeignKey(nameof(OrderDetailId))]
        public PurchaseOrderDetail? Detail { get; set; }

        [Required]
        public int TaxId { get; set; }

        [ForeignKey(nameof(TaxId))]
        public Tax? Tax { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal IGSTRate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal CGSTRate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal SGSTRate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal CessRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxableAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal IGSTAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CGSTAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SGSTAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CessAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalTaxAmount { get; set; }
    }
}
