using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class PurchaseInvoiceDetail
    {
        [Key]
        public int DetailId { get; set; }
        public int InvoiceId { get; set; }   // FK → PurchaseInvoiceMain

        // Item Info
        public int ItemId { get; set; }
        public int HsnId { get; set; }
        public string HsnCode { get; set; } = string.Empty;

        // Pricing
        public string PriceType { get; set; } = string.Empty;  // MRP / Retail / Wholesale / Purchase
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }                       // Purchase rate (excl. tax if IsTaxIncluded=false)
        public decimal DiscountRate { get; set; } = 0;
        public decimal AddisDiscountRate { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal AddisDiscountAmount { get; set; } = 0;
        public bool IsTaxIncluded { get; set; }

        // Amounts
        public decimal TaxableAmount { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }
        public decimal LineTaxAmount { get; set; }
        public decimal LineTotal { get; set; }

        // Navigation
        public PurchaseInvoiceMain? Invoice { get; set; }
        public Item? Item { get; set; }
        public Hsn? Hsn { get; set; }
        public List<PurchaseInvoiceTaxDetail> TaxDetails { get; set; } = new();

        public ICollection<PurchaseInvoiceDetailBatch>? Batches { get; set; }
        public ICollection<PurchaseInvoiceDetailSerial>? Serials { get; set; }
    }
}
