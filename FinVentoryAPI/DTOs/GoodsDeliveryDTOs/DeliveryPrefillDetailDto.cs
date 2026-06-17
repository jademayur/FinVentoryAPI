namespace FinVentoryAPI.DTOs.GoodsDeliveryDTOs
{
    public class DeliveryPrefillDetailDto
    {
        public int OrderId { get; set; }
        public string OrderNo { get; set; } = string.Empty;
        public int OrderDetailId { get; set; }

        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public int HsnId { get; set; }
        public string HsnCode { get; set; } = string.Empty;
        public string PriceType { get; set; } = string.Empty;

        public decimal OrderedQty { get; set; }
        public decimal PreviouslyDeliveredQty { get; set; }
        public decimal PendingQty { get; set; }
        public decimal SuggestedDeliveryQty { get; set; }

        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public bool IsTaxIncluded { get; set; }

        // ✅ ADD THESE — needed by Angular form for client-side tax preview
        public decimal CgstRate { get; set; }
        public decimal SgstRate { get; set; }
        public decimal IgstRate { get; set; }
        public decimal CessRate { get; set; }

        public bool IsManualTax { get; set; }
        public int? ManualTaxId { get; set; }
    }
}
