namespace FinVentoryAPI.DTOs.PurchaseReturnDTOs
{
    public class InvoicePickerDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string? SupplierInvoiceNo { get; set; }
        public decimal NetTotal { get; set; }
        public List<InvoicePickerDetailDto> Details { get; set; } = new();
    }
}
