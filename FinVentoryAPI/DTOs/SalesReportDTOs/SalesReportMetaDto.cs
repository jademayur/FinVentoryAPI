namespace FinVentoryAPI.DTOs.SalesReportDTOs
{
    public class SalesReportMetaDto
    {
        public decimal TotalSubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalCess { get; set; }
        public decimal TotalNet { get; set; }
        public int TotalRecords { get; set; }
    }
}
