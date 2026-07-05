namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class GRNPickerDetailDto
    {
        public int GRNDetailId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal InvoicedQty { get; set; }
        public decimal PendingQty { get; set; }
    }

}
