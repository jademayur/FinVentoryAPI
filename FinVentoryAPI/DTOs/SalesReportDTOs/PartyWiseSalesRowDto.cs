namespace FinVentoryAPI.DTOs.SalesReportDTOs
{
    public class PartyWiseSalesRowDto
    {
        public int BusinessPartnerId { get; set; }
        public string PartyName { get; set; } = "";
        public string? GstNo { get; set; }
        public string? GstType { get; set; }
        public int InvoiceCount { get; set; }
        public decimal TotalSubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalCess { get; set; }
        public decimal TotalNet { get; set; }
    }
}
