using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class PurchaseReturnDetail
    {
        [Key]
        public int DetailId { get; set; }
        public int ReturnId { get; set; }
        public int ItemId { get; set; }
        public int HsnId { get; set; }
        public string HsnCode { get; set; } = string.Empty;
        public string? PriceType { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal AddisDiscountRate { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AddisDiscountAmount { get; set; }
        public bool IsTaxIncluded { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }
        public decimal LineTaxAmount { get; set; }
        public decimal LineTotal { get; set; }

        public PurchaseReturnMain? Return { get; set; }
        public Item? Item { get; set; }
        public Hsn? Hsn { get; set; }
        public List<PurchaseReturnTaxDetail>? TaxDetails { get; set; }
        public List<PurchaseReturnDetailBatch>? Batches { get; set; }
        public List<PurchaseReturnDetailSerial>? Serials { get; set; }
    }
}
