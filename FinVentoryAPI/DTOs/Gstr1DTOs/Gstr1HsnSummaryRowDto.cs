namespace FinVentoryAPI.DTOs.Gstr1DTOs
{
    public class Gstr1HsnSummaryRowDto
    {
        public string HsnCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UqcCode { get; set; } = string.Empty;   // Unit of measure
        public decimal TotalQty { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTax => IGSTAmount + CGSTAmount + SGSTAmount + CessAmount;
    }
}
