namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class DeliveryPickerDetailDto
    {
        public int DeliveryDetailId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public string? HsnCode { get; set; }
        public string PriceType { get; set; } = string.Empty;

        public decimal DeliveredQty { get; set; }
        public decimal InvoicedQty { get; set; }
        public decimal PendingQty { get; set; }

        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public bool IsTaxIncluded { get; set; }
    }
}
