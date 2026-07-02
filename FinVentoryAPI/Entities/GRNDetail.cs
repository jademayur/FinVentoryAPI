using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class GRNDetail
    {
        [Key]
         public int GRNDetailId { get; set; }

        public int GRNId { get; set; }

        // Source Purchase Order
        public int? PurchaseOrderId { get; set; }
        public int? PurchaseOrderDetailId { get; set; }

        // Item
        public int ItemId { get; set; }
        public int? HsnId { get; set; }

        [MaxLength(30)]
        public string? HsnCode { get; set; }

        [MaxLength(20)]
        public string? PriceType { get; set; }

        // ── Quantities ───────────────────────────────────────────────────────
        public decimal OrderedQty { get; set; }
        public decimal PreviouslyReceivedQty { get; set; }
        public decimal ReceivedQty { get; set; }

        // ── Pricing ──────────────────────────────────────────────────────────
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AddisDiscountAmount { get; set; }
        public bool IsTaxIncluded { get; set; }

        // ── Tax financials ───────────────────────────────────────────────────
        public decimal TaxableAmount { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }
        public decimal LineTaxAmount { get; set; }
        public decimal LineTotal { get; set; }

        // ── Navigation ───────────────────────────────────────────────────────
        [ForeignKey(nameof(GRNId))]
        public GRNMain? GRN { get; set; }

        [ForeignKey(nameof(PurchaseOrderId))]
        public PurchaseOrderMain? PurchaseOrder { get; set; }

        [ForeignKey(nameof(PurchaseOrderDetailId))]
        public PurchaseOrderDetail? PurchaseOrderDetail { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        [ForeignKey(nameof(HsnId))]
        public Hsn? Hsn { get; set; }

        public List<GRNTaxDetail>? TaxDetails { get; set; }
    }
}
