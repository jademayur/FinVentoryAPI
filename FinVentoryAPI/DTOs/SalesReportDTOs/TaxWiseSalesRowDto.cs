namespace FinVentoryAPI.DTOs.SalesReportDTOs
{
    public class TaxWiseSalesRowDto
    {
        public string TaxName { get; set; } = "";
        public decimal TaxableAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetAmount { get; set; }
    }
}
