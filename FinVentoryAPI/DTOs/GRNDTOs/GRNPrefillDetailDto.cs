namespace FinVentoryAPI.DTOs.GRNDTOs
{
    public class GRNPrefillDetailDto
    {
        public int PurchaseOrderId { get; set; }
        public string PurchaseOrderNo { get; set; } = string.Empty;
        public int PurchaseOrderDetailId { get; set; }

        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }

        public int? HsnId { get; set; }
        public string? HsnCode { get; set; }
        public string? PriceType { get; set; }

        public decimal OrderedQty { get; set; }
        public decimal PreviouslyReceivedQty { get; set; }
        public decimal PendingQty { get; set; }
        public decimal SuggestedReceivedQty { get; set; }   // = PendingQty by default

        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public bool IsTaxIncluded { get; set; }

        // Tax rates for live frontend recalculation
        public decimal CgstRate { get; set; }
        public decimal SgstRate { get; set; }
        public decimal IgstRate { get; set; }
        public decimal CessRate { get; set; }
    }
}
