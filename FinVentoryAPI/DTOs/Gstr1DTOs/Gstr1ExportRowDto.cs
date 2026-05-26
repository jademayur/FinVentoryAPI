namespace FinVentoryAPI.DTOs.Gstr1DTOs
{
    public class Gstr1ExportRowDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string PortCode { get; set; } = string.Empty;
        public string ShippingBillNo { get; set; } = string.Empty;
        public DateTime? ShippingBillDate { get; set; }
        public string ExportType { get; set; } = string.Empty;  // "WithPayment" | "WithoutPayment"
        public decimal InvoiceValue { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
    }
}
