namespace FinVentoryAPI.DTOs.Gstr9DTOs
{
    public class Gstr9TotalsDto
    {
        public decimal TotalOutwardSupplies { get; set; }
        public decimal TotalInwardSupplies { get; set; }
        public decimal TotalITCAvailed { get; set; }
        public decimal TotalTaxPayable { get; set; }
        public decimal TotalTaxPaidViaITC { get; set; }
        public decimal TotalTaxPaidViaCash { get; set; }
        public decimal TotalIGST { get; set; }
        public decimal TotalCGST { get; set; }
        public decimal TotalSGST { get; set; }
        public decimal TotalCess { get; set; }
        public decimal TotalTax => TotalIGST + TotalCGST + TotalSGST + TotalCess;
        public int TotalSalesInvoices { get; set; }
        public int TotalPurchaseInvoices { get; set; }
    }
}
