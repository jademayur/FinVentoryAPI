namespace FinVentoryAPI.DTOs.Gstr1DTOs
{
    public class Gstr1B2CSRowDto
    {
        public int? PlaceOfSupply { get; set; }
        public string StateName { get; set; } = string.Empty;
        public bool IsInterState { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTax => IGSTAmount + CGSTAmount + SGSTAmount + CessAmount;
    }
}
