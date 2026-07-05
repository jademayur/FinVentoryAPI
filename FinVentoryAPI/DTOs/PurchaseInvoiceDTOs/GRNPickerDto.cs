namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class GRNPickerDto
    {
        public int GRNId { get; set; }
        public string GRNNo { get; set; } = string.Empty;
        public DateTime GRNDate { get; set; }
        public string? SupplierInvoiceNo { get; set; }
        public decimal NetTotal { get; set; }
        public List<GRNPickerDetailDto> Details { get; set; } = new();
    }
}
