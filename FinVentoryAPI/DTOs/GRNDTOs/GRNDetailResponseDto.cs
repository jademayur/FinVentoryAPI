namespace FinVentoryAPI.DTOs.GRNDTOs
{
    public class GRNDetailResponseDto
    {
        public int GRNDetailId { get; set; }
        public int GRNId { get; set; }

        public int? PurchaseOrderId { get; set; }
        public string? PurchaseOrderNo { get; set; }
        public int? PurchaseOrderDetailId { get; set; }

        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }

        public int? HsnId { get; set; }
        public string? HsnCode { get; set; }
        public string? PriceType { get; set; }

        public decimal OrderedQty { get; set; }
        public decimal PreviouslyReceivedQty { get; set; }
        public decimal ReceivedQty { get; set; }

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

        public List<GRNTaxDetailResponseDto> TaxDetails { get; set; } = new();
    }
}
