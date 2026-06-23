namespace FinVentoryAPI.DTOs.Gstr1DTOs
{
    public class Gstr1HsnRowDto
    {
        public string HSNCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string UOM { get; set; } = string.Empty;
        public decimal TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }       // invoice value (NetTotal contribution)
        public decimal TaxableValue { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
    }
}
