namespace FinVentoryAPI.DTOs.SalesReportDTOs
{
    public class SalesRegisterRowDto
    {
        public string InvoiceNo { get; set; } = "";
        public string InvoiceDate { get; set; } = "";
        public string PartyName { get; set; } = "";
        public string? GstNo { get; set; }
        public string? GstType { get; set; }
        public string? SalesPerson { get; set; }
        public string? Location { get; set; }
        public string Status { get; set; } = "";
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal NetTotal { get; set; }
        public string? Remarks { get; set; }
    }
}
