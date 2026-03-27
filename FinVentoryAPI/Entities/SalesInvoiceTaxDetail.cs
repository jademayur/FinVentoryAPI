namespace FinVentoryAPI.Entities
{
    public class SalesInvoiceTaxDetail
    {
        public int TaxDetailId { get; set; }
        public int InvoiceId { get; set; }   // FK → SalesInvoiceMain
        public int DetailId { get; set; }    // FK → SalesInvoiceDetail
        public int TaxId { get; set; }       // FK → Tax

        // Tax Rates — copied from Tax entity at time of save
        public decimal IGSTRate { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal SGSTRate { get; set; }

        // Tax Amounts
        public decimal TaxableAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }

        //Posting Accounts — copied from Tax entity at time of save
        public int? IGSTPostingAccountId { get; set; }  // copied from Tax.IGSTPostingAccountId
        public int? CGSTPostingAccountId { get; set; }  // copied from Tax.CGSTPostingAccountId
        public int? SGSTPostingAccountId { get; set; }  // copied from Tax.SGSTPostingAccountId


        // Cess — copied from Hsn entity at time of save
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }
        public int? CessPostingAccountId { get; set; }  // copied from Hsn.CessPostingAc

        // Total
        public decimal TotalTaxAmount { get; set; }  // IGST+CGST+SGST+Cess

        // Navigation
        public SalesInvoiceMain? Invoice { get; set; }
        public SalesInvoiceDetail? Detail { get; set; }
        public Tax? Tax { get; set; }
        public Account? CessPostingAccount { get; set; }
    }
}
