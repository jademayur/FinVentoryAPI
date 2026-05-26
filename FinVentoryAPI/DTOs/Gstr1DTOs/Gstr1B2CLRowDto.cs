namespace FinVentoryAPI.DTOs.Gstr1DTOs
{
    public class Gstr1B2CLRowDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public int? PlaceOfSupply { get; set; }
        public decimal InvoiceValue { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTax => IGSTAmount + CessAmount;
    }
}
