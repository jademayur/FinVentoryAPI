using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class SalesOrderTaxDetail
    {
        [Key]
        public int TaxDetailId { get; set; }
        public int OrderId { get; set; }
        public int OrderDetailId { get; set; }
        public int TaxId { get; set; }

        public decimal IGSTRate { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal CessRate { get; set; }

        public decimal TaxableAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }

        // Navigation
        public SalesOrderMain? Order { get; set; }
        public SalesOrderDetail? Detail { get; set; }
        public Tax? Tax { get; set; }
    }
}
