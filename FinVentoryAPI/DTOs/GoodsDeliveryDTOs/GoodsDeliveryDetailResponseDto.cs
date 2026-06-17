namespace FinVentoryAPI.DTOs.GoodsDeliveryDTOs
{
    public class GoodsDeliveryDetailResponseDto
    {
        public int DeliveryDetailId { get; set; }
        public int DeliveryId { get; set; }

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
        public decimal DeliveryQty { get; set; }

        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AddisDiscountAmount { get; set; }
        public bool IsTaxIncluded { get; set; }

        public decimal TaxableAmount { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }
        public decimal LineTaxAmount { get; set; }
        public decimal LineTotal { get; set; }

        public List<GoodsDeliveryTaxDetailResponseDto> TaxDetails { get; set; } = new();
    }
}
