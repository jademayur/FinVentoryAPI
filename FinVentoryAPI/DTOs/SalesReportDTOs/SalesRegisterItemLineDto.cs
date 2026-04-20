namespace FinVentoryAPI.DTOs.SalesReportDTOs
{
    public class SalesRegisterItemLineDto
    {
        public string ItemName { get; set; } = "";
        public string? ItemCode { get; set; }
        public string? HsnCode { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountAmt { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal LineTotal { get; set; }
    }
}
