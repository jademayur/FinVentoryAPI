namespace FinVentoryAPI.DTOs.GRNDTOs
{
    public class PurchaseOrderPickerDetailDto
    {
        public int PurchaseOrderDetailId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public string? HsnCode { get; set; }
        public string? PriceType { get; set; }
        public decimal OrderedQty { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal PendingQty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public bool IsTaxIncluded { get; set; }
    }
}
