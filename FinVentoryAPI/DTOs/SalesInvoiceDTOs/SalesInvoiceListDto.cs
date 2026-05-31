namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class SalesInvoiceListDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal NetTotal { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<SalesInvoiceListDetailDto> Details { get; set; } = new();
    }
}
