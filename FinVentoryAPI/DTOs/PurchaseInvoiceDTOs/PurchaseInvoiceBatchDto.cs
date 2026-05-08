using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class PurchaseInvoiceBatchDto
    {
        [Required(ErrorMessage = "Batch is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Batch.")]
        public int BatchId { get; set; }

        [Required(ErrorMessage = "Batch quantity is required.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Batch quantity must be greater than 0.")]
        public decimal Qty { get; set; }
    }
}
