using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.GRNDTOs
{
    public class UpdateGRNDetailDto
    {
        public int? PurchaseOrderId { get; set; }
        public int? PurchaseOrderDetailId { get; set; }

        [Required]
        public int ItemId { get; set; }

        public string? PriceType { get; set; }

        [Range(0.001, double.MaxValue)]
        public decimal ReceivedQty { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Rate { get; set; }

        [Range(0, 100)]
        public decimal DiscountRate { get; set; }

        [Range(0, 100)]
        public decimal AddisDiscountRate { get; set; }

        public bool IsTaxIncluded { get; set; }
    }
}
