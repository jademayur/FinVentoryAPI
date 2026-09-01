namespace FinVentoryAPI.DTOs.PurchaseReturnDTOs
{
    public class ReturnPrefillDetailDto
    {
        public int InvoiceDetailId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public int HsnId { get; set; }
        public string HsnCode { get; set; } = string.Empty;
        public string PriceType { get; set; } = string.Empty;
        public decimal InvoiceQty { get; set; }
        public decimal AlreadyReturnedQty { get; set; }
        public decimal PendingReturnQty { get; set; }
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
