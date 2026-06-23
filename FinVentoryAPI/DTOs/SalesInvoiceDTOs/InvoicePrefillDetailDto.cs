namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class InvoicePrefillDetailDto
    {
        public int DeliveryId { get; set; }
        public string DeliveryNo { get; set; } = string.Empty;
        public int DeliveryDetailId { get; set; }

        public int OrderId { get; set; }
        public int? OrderDetailId { get; set; }

        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }

        public int HsnId { get; set; }
        public string? HsnCode { get; set; }

        public string PriceType { get; set; } = string.Empty;

        public decimal DeliveredQty { get; set; }
        public decimal PreviouslyInvoicedQty { get; set; }
        public decimal PendingQty { get; set; }
        public decimal SuggestedQty { get; set; }

        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public bool IsTaxIncluded { get; set; }

        public decimal CgstRate { get; set; }
        public decimal SgstRate { get; set; }
        public decimal IgstRate { get; set; }
        public decimal CessRate { get; set; }

        public bool IsManualTax { get; set; }
        public int? ManualTaxId { get; set; }
    }
}
