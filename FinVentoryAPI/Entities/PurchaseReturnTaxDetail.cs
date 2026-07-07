using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class PurchaseReturnTaxDetail
    {
        [Key]
        public int TaxDetailId { get; set; }
        public int ReturnId { get; set; }
        public int DetailId { get; set; }
        public int TaxId { get; set; }
        public decimal IGSTRate { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public int? IGSTPostingAccountId { get; set; }
        public int? CGSTPostingAccountId { get; set; }
        public int? SGSTPostingAccountId { get; set; }
        public int? CessPostingAccountId { get; set; }

        public PurchaseReturnMain? Return { get; set; }
        public PurchaseReturnDetail? Detail { get; set; }
        public Tax? Tax { get; set; }
        public Account? IGSTPostingAccount { get; set; }
        public Account? CGSTPostingAccount { get; set; }
        public Account? SGSTPostingAccount { get; set; }
        public Account? CessPostingAccount { get; set; }
    }
}
