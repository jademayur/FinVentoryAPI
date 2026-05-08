using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class CreatePurchaseInvoiceDetailDto
    {
        [Required(ErrorMessage = "Item is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Item.")]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Price Type is required.")]
        [MaxLength(50, ErrorMessage = "Price Type cannot exceed 50 characters.")]
        public string PriceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Qty { get; set; }

        [Required(ErrorMessage = "Rate is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Rate must be 0 or greater.")]
        public decimal Rate { get; set; }

        [Range(0, 100, ErrorMessage = "Discount Rate must be between 0 and 100.")]
        public decimal DiscountRate { get; set; } = 0;

        [Range(0, 100, ErrorMessage = "Additional Discount Rate must be between 0 and 100.")]
        public decimal AddisDiscountRate { get; set; } = 0;

        public bool IsTaxIncluded { get; set; }

        // Batch / Serial allocations (required when item is Batch/Serial managed)
        public List<PurchaseInvoiceBatchDto>? Batches { get; set; }
        public List<PurchaseInvoiceSerialDto>? Serials { get; set; }
    }
}
