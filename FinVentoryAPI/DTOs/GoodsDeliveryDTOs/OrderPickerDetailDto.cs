namespace FinVentoryAPI.DTOs.GoodsDeliveryDTOs
{
    public class OrderPickerDetailDto
    {
        public int OrderDetailId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public string HsnCode { get; set; } = string.Empty;
        public string PriceType { get; set; } = string.Empty;
        public decimal OrderedQty { get; set; }
        public decimal DeliveredQty { get; set; }
        public decimal PendingQty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public bool IsTaxIncluded { get; set; }
    }
}
