namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class InvoiceForReturnDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public int BusinessPartnerId { get; set; }
        public int SalesAccountId { get; set; }
        public int? SalesStateCode { get; set; }
        public int? BillStateCode { get; set; }
        public List<InvoiceForReturnDetailDto> Details { get; set; } = new();
    }
}
