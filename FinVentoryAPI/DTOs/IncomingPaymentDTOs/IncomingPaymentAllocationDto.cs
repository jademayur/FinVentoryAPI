using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.IncomingPaymentDTOs
{
    public class IncomingPaymentAllocationDto
    {

        /// <summary>SalesInvoiceMain.InvoiceId being settled.</summary>
        [Required(ErrorMessage = "Invoice ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invoice ID must be a positive integer.")]
        public int InvoiceId { get; set; }

        /// <summary>Invoice number (display / validation only).</summary>
        [Required(ErrorMessage = "Invoice number is required.")]
        [StringLength(50, ErrorMessage = "Invoice number cannot exceed 50 characters.")]
        public string InvoiceNo { get; set; } = string.Empty;

        /// <summary>Amount applied against this specific invoice.</summary>
        [Required(ErrorMessage = "Amount applied is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount applied must be greater than zero.")]
        public decimal AmountApplied { get; set; }
    }
}
