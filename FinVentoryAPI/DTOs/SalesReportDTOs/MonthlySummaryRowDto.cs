namespace FinVentoryAPI.DTOs.SalesReportDTOs
{
    public class MonthlySummaryRowDto
    {
        public string MonthLabel { get; set; } = "";
        public int Year { get; set; }
        public int Month { get; set; }
        public int InvoiceCount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal NetTotal { get; set; }
    }
}
