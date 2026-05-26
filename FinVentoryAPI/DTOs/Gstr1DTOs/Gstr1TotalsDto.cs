namespace FinVentoryAPI.DTOs.Gstr1DTOs
{
    public class Gstr1TotalsDto
    {
        public decimal TotalTaxableValue { get; set; }
        public decimal TotalIGST { get; set; }
        public decimal TotalCGST { get; set; }
        public decimal TotalSGST { get; set; }
        public decimal TotalCess { get; set; }
        public decimal TotalTax => TotalIGST + TotalCGST + TotalSGST + TotalCess;
        public decimal TotalInvoiceValue { get; set; }
        public int TotalInvoices { get; set; }
        public int TotalCDNR { get; set; }
        public int TotalCDNUR { get; set; }
        public int TotalExports { get; set; }
    }
}
