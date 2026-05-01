using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class SalesInvoiceDetail
    {
        [Key]
        public int DetailId { get; set; }
        public int InvoiceId { get; set; }  // FK → SalesInvoiceMain

        // Item Info
        public int ItemId { get; set; }
        public int HsnId { get; set; }               // FK → Hsn (saved at time of invoice)
        public string HsnCode { get; set; } = string.Empty;  // copied from Hsn (for history)

        // Pricing
        public string PriceType { get; set; } = string.Empty;  // MRP / Retail / Wholesale
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; } = 0;
        public decimal AddisDiscountRate { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal AddisDiscountAmount { get; set; } = 0;
        public bool IsTaxIncluded { get; set; }      // copied from ItemPrice

        // Amounts
        public decimal TaxableAmount { get; set; }   // (Rate * Qty) - Discount
        public decimal CessRate { get; set; }        // copied from Hsn.Cess
        public decimal CessAmount { get; set; }      // TaxableAmount * CessRate / 100
        public decimal LineTaxAmount { get; set; }   // IGST/CGST/SGST + Cess
        public decimal LineTotal { get; set; }       // TaxableAmount + LineTaxAmount

        // Navigation
        public SalesInvoiceMain? Invoice { get; set; }
        public Item? Item { get; set; }
        public Hsn? Hsn { get; set; }
        //public ICollection<SalesInvoiceTaxDetail>? TaxDetails { get; set; }
        public List<SalesInvoiceTaxDetail> TaxDetails { get; set; }

        public ICollection<SalesInvoiceDetailBatch>? Batches { get; set; }
        public ICollection<SalesInvoiceDetailSerial>? Serials { get; set; }
    }
}
