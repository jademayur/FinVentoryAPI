namespace FinVentoryAPI.DTOs.GoodsDeliveryDTOs
{
    public class CreateGoodsDeliveryDetailDto
    {
        /// <summary>Source sales order for this line.</summary>
        public int OrderId { get; set; }

        /// <summary>Source order detail line (required for partial delivery tracking).</summary>
        public int OrderDetailId { get; set; }

        public int ItemId { get; set; }
        public string PriceType { get; set; } = string.Empty;

        /// <summary>Qty the user actually wants to deliver (may be less than pending qty).</summary>
        public decimal DeliveryQty { get; set; }

        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public bool IsTaxIncluded { get; set; }

        public int? ManualTaxId { get; set; }
        public decimal? ManualIgstRate { get; set; }
        public decimal? ManualCgstRate { get; set; }
        public decimal? ManualSgstRate { get; set; }
        public decimal? ManualCessRate { get; set; }
    }
}
