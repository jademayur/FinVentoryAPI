using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class CreateSalesInvoiceDetailDto
    {
        [Required(ErrorMessage = "Item is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Item.")]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Price Type is required.")]
        [MaxLength(50, ErrorMessage = "Price Type cannot exceed 50 characters.")]
        public string PriceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Qty { get; set; }

        [Required(ErrorMessage = "Rate is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Rate cannot be negative.")]
        public decimal Rate { get; set; }

        [Range(0, 100, ErrorMessage = "Discount Rate must be between 0 and 100.")]
        public decimal DiscountRate { get; set; } = 0;

        [Range(0, 100, ErrorMessage = "Additional Discount Rate must be between 0 and 100.")]
        public decimal AddisDiscountRate { get; set; } = 0;

        public bool IsTaxIncluded { get; set; }

        // ── Batch / Serial ────────────────────────────────
        // Required only when Item.ItemManageBy == Batch.
        // Sum of Qty across all entries must equal Qty above.
        public List<SalesInvoiceDetailBatchDto>? Batches { get; set; }

        // Required only when Item.ItemManageBy == Serial.
        // Count must equal Qty above (each serial = 1 unit).
        public List<SalesInvoiceDetailSerialDto>? Serials { get; set; }
    }
}
