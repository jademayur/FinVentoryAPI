namespace FinVentoryAPI.DTOs.Gstr1DTOs
{
    public class Gstr1B2BRowDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string RecipientGSTIN { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public int? PlaceOfSupply { get; set; }
        public bool IsInterState { get; set; }
        public bool IsReverseCharge { get; set; }
        public decimal InvoiceValue { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTax => IGSTAmount + CGSTAmount + SGSTAmount + CessAmount;
    }
}
