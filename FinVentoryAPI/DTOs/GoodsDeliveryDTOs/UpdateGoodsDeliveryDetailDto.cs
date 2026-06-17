namespace FinVentoryAPI.DTOs.GoodsDeliveryDTOs
{
    public class UpdateGoodsDeliveryDetailDto
    {
        public int OrderId { get; set; }
        public int OrderDetailId { get; set; }
        public int ItemId { get; set; }
        public string PriceType { get; set; } = string.Empty;
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
