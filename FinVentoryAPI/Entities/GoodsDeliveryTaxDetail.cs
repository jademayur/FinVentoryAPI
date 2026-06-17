using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class GoodsDeliveryTaxDetail
    {
        [Key]
        public int TaxDetailId { get; set; }
        public int DeliveryId { get; set; }
        public int DeliveryDetailId { get; set; }
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

        public GoodsDeliveryMain? Delivery { get; set; }
        public GoodsDeliveryDetail? Detail { get; set; }
        public Tax? Tax { get; set; }
    }
}
