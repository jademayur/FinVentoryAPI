namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class InvoicePrefillDetailDto
    {
        public int GRNId { get; set; }
        public string GRNNo { get; set; } = string.Empty;
        public int GRNDetailId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public int HsnId { get; set; }
        public string HsnCode { get; set; } = string.Empty;
        public string PriceType { get; set; } = string.Empty;
        public decimal ReceivedQty { get; set; }
        public decimal PreviouslyInvoicedQty { get; set; }
        public decimal PendingQty { get; set; }
        public decimal SuggestedQty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public bool IsTaxIncluded { get; set; }
        public decimal IgstRate { get; set; }
        public decimal CgstRate { get; set; }
        public decimal SgstRate { get; set; }
        public decimal CessRate { get; set; }
        public string ItemManageBy { get; set; } = "Regular";
    }
}
