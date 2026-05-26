namespace FinVentoryAPI.DTOs.Gstr9DTOs
{
    public class Gstr9MonthlyBreakdownDto
    {
        public string Month { get; set; } = string.Empty;   // "Apr-2024"
        public decimal TaxableValue { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTax => IGSTAmount + CGSTAmount + SGSTAmount + CessAmount;
        public decimal NetTotal { get; set; }
        public int InvoiceCount { get; set; }
    }
}
