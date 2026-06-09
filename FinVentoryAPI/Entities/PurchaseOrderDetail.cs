using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class PurchaseOrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public PurchaseOrderMain? Order { get; set; }

        // Item
        [Required]
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        // HSN
        public int HsnId { get; set; }

        [ForeignKey(nameof(HsnId))]
        public Hsn? Hsn { get; set; }

        [MaxLength(30)]
        public string? HsnCode { get; set; }

        [MaxLength(10)]
        public string? PriceType { get; set; }   // e.g. "MRP", "Wholesale"

        [Column(TypeName = "decimal(18,4)")]
        public decimal Qty { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal DiscountRate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal AddisDiscountRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AddisDiscountAmount { get; set; }

        public bool IsTaxIncluded { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxableAmount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal CessRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CessAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        // Navigation
        public List<PurchaseOrderTaxDetail>? TaxDetails { get; set; }
    }
}
