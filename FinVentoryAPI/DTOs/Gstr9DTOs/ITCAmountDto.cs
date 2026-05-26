namespace FinVentoryAPI.DTOs.Gstr9DTOs
{
    public class ITCAmountDto
    {
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalITC => IGSTAmount + CGSTAmount + SGSTAmount + CessAmount;
    }
}
