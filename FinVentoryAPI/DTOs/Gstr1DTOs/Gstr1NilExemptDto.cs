namespace FinVentoryAPI.DTOs.Gstr1DTOs
{
    public class Gstr1NilExemptDto
    {
        public decimal NilRatedValue { get; set; }
        public decimal ExemptValue { get; set; }
        public decimal NonGstValue { get; set; }
        public decimal Total => NilRatedValue + ExemptValue + NonGstValue;
    }
}
