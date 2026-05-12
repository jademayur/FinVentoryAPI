using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.OutgoingPaymentDTOs
{
    public class UpdateOutgoingPaymentDto
    {
        [Required(ErrorMessage = "Payment date is required.")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Business Partner is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Business Partner ID must be greater than zero.")]
        public int BusinessPartnerId { get; set; }

        [Required(ErrorMessage = "Payment Account is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Payment Account ID must be greater than zero.")]
        public int PaymentAccountId { get; set; }

        [Required(ErrorMessage = "Payment mode is required.")]
        [StringLength(20, ErrorMessage = "Payment mode cannot exceed 20 characters.")]
        public string PaymentMode { get; set; } = "Cash";

        [StringLength(50, ErrorMessage = "Cheque number cannot exceed 50 characters.")]
        public string? ChequeNo { get; set; }

        public DateTime? ChequeDate { get; set; }

        [StringLength(100, ErrorMessage = "Bank name cannot exceed 100 characters.")]
        public string? BankName { get; set; }

        [StringLength(100, ErrorMessage = "Transaction reference cannot exceed 100 characters.")]
        public string? TransactionRef { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }

        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than zero.")]
        public decimal TotalAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "On-account amount cannot be negative.")]
        public decimal OnAccountAmount { get; set; }

        [Required(ErrorMessage = "Allocations are required.")]
        public List<OutgoingPaymentAllocationDto> Allocations { get; set; } = new();
    }
}
