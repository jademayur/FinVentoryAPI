namespace FinVentoryAPI.DTOs.SalesReportDTOs
{
    public class ItemWiseSalesRowDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public string? ItemCode { get; set; }
        public string? HsnCode { get; set; }
        public int InvoiceCount { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTaxable { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalCess { get; set; }
        public decimal TotalNet { get; set; }
    }
}
