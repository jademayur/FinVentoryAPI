namespace FinVentoryAPI.DTOs.Gstr9DTOs
{
    public class SupplyAmountDto
    {
        public decimal TaxableValue { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTax => IGSTAmount + CGSTAmount + SGSTAmount + CessAmount;
    }
}
