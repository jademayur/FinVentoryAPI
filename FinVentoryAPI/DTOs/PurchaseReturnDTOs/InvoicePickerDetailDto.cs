namespace FinVentoryAPI.DTOs.PurchaseReturnDTOs
{
    public class InvoicePickerDetailDto
    {
        public int DetailId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public decimal InvoiceQty { get; set; }
        public decimal AlreadyReturnedQty { get; set; }
        public decimal PendingReturnQty { get; set; }
    }
}
