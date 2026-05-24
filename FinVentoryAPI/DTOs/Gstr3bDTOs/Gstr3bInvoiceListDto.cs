namespace FinVentoryAPI.DTOs.Gstr3bDTOs
{
    public class Gstr3bInvoiceListDto
    {
        public string InvoiceType { get; set; } = string.Empty;  // "Sales" | "Purchase"
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string PartyGstin { get; set; } = string.Empty;
        public decimal TaxableAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal NetTotal { get; set; }
        public bool IsInterState { get; set; }
    }
}
