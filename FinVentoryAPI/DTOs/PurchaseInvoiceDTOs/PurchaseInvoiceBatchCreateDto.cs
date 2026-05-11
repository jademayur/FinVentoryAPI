using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class PurchaseInvoiceBatchCreateDto
    {
        [Required(ErrorMessage = "Batch No is required.")]
        [MaxLength(100, ErrorMessage = "Batch No cannot exceed 100 characters.")]
        public string BatchNo { get; set; } = string.Empty;

        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        [Required(ErrorMessage = "Batch quantity is required.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Batch quantity must be greater than 0.")]
        public decimal Qty { get; set; }
    }
}
