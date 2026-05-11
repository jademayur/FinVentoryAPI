using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class UpdatePurchaseInvoiceDetailDto
    {
        [Required(ErrorMessage = "Item is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Item.")]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Price Type is required.")]
        [MaxLength(50)]
        public string PriceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Qty { get; set; }

        [Range(0, double.MaxValue)] public decimal Rate { get; set; }
        [Range(0, 100)] public decimal DiscountRate { get; set; }
        [Range(0, 100)] public decimal AddisDiscountRate { get; set; }
        public bool IsTaxIncluded { get; set; }

        public List<PurchaseInvoiceBatchCreateDto>? Batches { get; set; }
        public List<PurchaseInvoiceSerialCreateDto>? Serials { get; set; }
    }
}
