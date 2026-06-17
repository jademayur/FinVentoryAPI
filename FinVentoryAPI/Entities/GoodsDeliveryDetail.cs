using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class GoodsDeliveryDetail
    {
        [Key]
        public int DeliveryDetailId { get; set; }
        public int DeliveryId { get; set; }

        // ── Source order line ─────────────────────────────────────
        /// <summary>The parent sales order this line was copied from.</summary>
        public int OrderId { get; set; }

        /// <summary>The specific order detail line (nullable: user may add an ad-hoc line).</summary>
        public int? OrderDetailId { get; set; }

        // ── Item ─────────────────────────────────────────────────
        public int ItemId { get; set; }
        public int HsnId { get; set; }
        public string HsnCode { get; set; } = string.Empty;
        public string PriceType { get; set; } = string.Empty;

        // ── Quantities ───────────────────────────────────────────
        /// <summary>Ordered qty on the source order detail line.</summary>
        public decimal OrderedQty { get; set; }

        /// <summary>Total qty already delivered against this order detail line (before this delivery).</summary>
        public decimal PreviouslyDeliveredQty { get; set; }

        /// <summary>Qty being delivered in THIS note.</summary>
        public decimal DeliveryQty { get; set; }

        // ── Pricing ──────────────────────────────────────────────
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AddisDiscountAmount { get; set; }
        public bool IsTaxIncluded { get; set; }

        // ── Calculated amounts ───────────────────────────────────
        public decimal TaxableAmount { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }
        public decimal LineTaxAmount { get; set; }
        public decimal LineTotal { get; set; }

        // ── Navigation ───────────────────────────────────────────
        public GoodsDeliveryMain? Delivery { get; set; }
        public SalesOrderMain? Order { get; set; }
        public SalesOrderDetail? OrderDetail { get; set; }
        public Item? Item { get; set; }
        public Hsn? Hsn { get; set; }
        public List<GoodsDeliveryTaxDetail>? TaxDetails { get; set; }
    }
}
